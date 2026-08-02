using System.Text;
using System.Text.Json;
using _2_Payment.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace _3_Payment.Infrastructure.Messaging;

public sealed class UsuarioRegistradoWorker : BackgroundService
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<UsuarioRegistradoWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private IConnection? _connection;
    private IChannel? _channel;

    private const string ExchangeName = "notificacao.exchange";
    private const string QueueName = "pagamento.usuario.registrado";
    private const string RoutingKey = "autenticacao.usuario.registrado";
    private const string DlxExchangeName = "pagamento.usuario.registrado.dlx.exchange";
    private const string DlqQueueName = "pagamento.usuario.registrado.dlq";
    private const string DlxRoutingKey = "pagamento.usuario.registrado.falha";

    public UsuarioRegistradoWorker(
        IConnectionFactory connectionFactory,
        ILogger<UsuarioRegistradoWorker> logger,
        IServiceScopeFactory scopeFactory)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Conectando ao RabbitMQ para consumir usuarios registrados...");
                _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken: stoppingToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await ConfigurarTopologiaAsync(stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += OnMessageReceivedAsync;

                await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

                _logger.LogInformation("Worker de provisionamento aguardando usuarios registrados na fila {Queue}.", QueueName);

                while (!stoppingToken.IsCancellationRequested && _connection.IsOpen && _channel.IsOpen)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao conectar ao RabbitMQ. Tentando novamente em 5 segundos...");
            }
            finally
            {
                await DisposeChannelAndConnectionAsync();
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ConfigurarTopologiaAsync(CancellationToken cancellationToken)
    {
        await _channel!.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: cancellationToken);
        await _channel.ExchangeDeclareAsync(DlxExchangeName, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(DlqQueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(DlqQueueName, DlxExchangeName, DlxRoutingKey, cancellationToken: cancellationToken);

        var mainQueueArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = DlxExchangeName,
            ["x-dead-letter-routing-key"] = DlxRoutingKey
        };

        await _channel.QueueDeclareAsync(
            QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: mainQueueArguments,
            cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey, cancellationToken: cancellationToken);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var evento = JsonSerializer.Deserialize<UsuarioRegistradoMensagem>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (evento is null)
            {
                throw new JsonException("Evento de usuario registrado nulo.");
            }

            await using var scope = _scopeFactory.CreateAsyncScope();
            var contaService = scope.ServiceProvider.GetRequiredService<IContaService>();
            var conta = await contaService.CriarContaAsync(evento.IdLogin);

            _logger.LogInformation(
                "Conta {ContaId} provisionada para o login {LoginId} ({Email}).",
                conta.IdConta,
                evento.IdLogin,
                evento.Email);

            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao provisionar conta do usuario registrado. Rejeitando mensagem.");
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }

    private async ValueTask DisposeChannelAndConnectionAsync()
    {
        if (_channel is not null)
        {
            try
            {
                await _channel.CloseAsync();
            }
            catch
            {
            }

            await _channel.DisposeAsync();
            _channel = null;
        }

        if (_connection is not null)
        {
            try
            {
                await _connection.CloseAsync();
            }
            catch
            {
            }

            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    private sealed record UsuarioRegistradoMensagem(
        int IdLogin,
        string Nome,
        string Email,
        int TipoUsuario);
}
