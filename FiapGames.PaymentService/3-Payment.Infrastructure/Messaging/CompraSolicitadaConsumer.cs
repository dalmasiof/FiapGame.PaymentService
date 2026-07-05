using Application.Interfaces;
using IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace Messaging;

public class CompraSolicitadaConsumer : BackgroundService
{
    private const string ExchangeName = "catalogo.exchange";
    private const string QueueName = "pagamento.compra.solicitada";
    private const string RoutingKey = "catalogo.compra.solicitada";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IConnectionFactory _connectionFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CompraSolicitadaConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public CompraSolicitadaConsumer(
        IConnectionFactory connectionFactory,
        IServiceScopeFactory scopeFactory,
        ILogger<CompraSolicitadaConsumer> logger)
    {
        _connectionFactory = connectionFactory;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndConsumeAsync(stoppingToken);
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consumir a fila {QueueName}. Nova tentativa em 5 segundos.", QueueName);
                await DisposeRabbitMqAsync();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ConnectAndConsumeAsync(CancellationToken cancellationToken)
    {
        _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: RoutingKey,
            cancellationToken: cancellationToken);

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += ProcessMessageAsync;

        await _channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Consumindo RabbitMQ: exchange={Exchange}, queue={Queue}, routingKey={RoutingKey}.",
            ExchangeName,
            QueueName,
            RoutingKey);
    }

    private async Task ProcessMessageAsync(object sender, BasicDeliverEventArgs args)
    {
        if (_channel is null) return;

        try
        {
            var evento = JsonSerializer.Deserialize<CompraSolicitadaIntegrationEvent>(args.Body.Span, JsonOptions);

            if (evento is null)
            {
                _logger.LogWarning("Mensagem vazia ou inválida recebida na fila {QueueName}.", QueueName);
                await _channel.BasicAckAsync(args.DeliveryTag, multiple: false);
                return;
            }

            await using var scope = _scopeFactory.CreateAsyncScope();
            var pagamentoService = scope.ServiceProvider.GetRequiredService<IPagamentoService>();

            await pagamentoService.ProcessarCompraSolicitadaAsync(evento);
            await _channel.BasicAckAsync(args.DeliveryTag, multiple: false);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Mensagem com JSON inválido recebida na fila {QueueName}.", QueueName);
            await _channel.BasicAckAsync(args.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem da fila {QueueName}.", QueueName);
            await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await DisposeRabbitMqAsync();
        await base.StopAsync(cancellationToken);
    }

    private async Task DisposeRabbitMqAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
            _channel = null;
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
