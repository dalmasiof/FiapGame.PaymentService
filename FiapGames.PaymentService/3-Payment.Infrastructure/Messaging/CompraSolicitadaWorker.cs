using System.Text;
using System.Text.Json;
using _2_Payment.Application.Interfaces;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace _3_Payment.Infrastructure.Messaging;

public sealed class CompraSolicitadaWorker : BackgroundService
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<CompraSolicitadaWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private IConnection? _connection;
    private IChannel? _channel;

    private const string ExchangeName = "catalogo.exchange";
    private const string QueueName = "pagamento.compra.solicitada";
    private const string RoutingKey = "catalogo.compra.solicitada";

    public CompraSolicitadaWorker(
        IConnectionFactory connectionFactory,
        ILogger<CompraSolicitadaWorker> logger,
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
                _logger.LogInformation("Conectando ao RabbitMQ para consumir solicitações de compra...");
                _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken: stoppingToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await ConfigurarTopologiaAsync(stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += OnMessageReceivedAsync;

                await _channel.BasicConsumeAsync(QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

                _logger.LogInformation("Worker de pagamento aguardando solicitações de compra na fila {Queue}.", QueueName);

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
        await _channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(QueueName, ExchangeName, RoutingKey, cancellationToken: cancellationToken);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var evento = JsonSerializer.Deserialize<CompraSolicitadaMensagem>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (evento is null)
            {
                throw new JsonException("Evento de compra nulo.");
            }

            _logger.LogInformation(
                "Solicitação de compra {CompraId} recebida para o usuário {UsuarioId}.",
                evento.CompraId,
                evento.UsuarioId);

            await using var scope = _scopeFactory.CreateAsyncScope();
            var contaRepository = scope.ServiceProvider.GetRequiredService<IContaRepository>();
            var pagamentoRepository = scope.ServiceProvider.GetRequiredService<IPagamentoRepository>();
            var messagingPublisher = scope.ServiceProvider.GetRequiredService<IMessagingPublisher>();

            var pagamento = new Pagamento(0, evento.CompraId);
            var aprovado = false;
            string? motivoRecusa = null;

            var conta = await contaRepository.ObterContaPorLoginId(evento.UsuarioId)
                ?? await contaRepository.ObterContaPorId(evento.UsuarioId);

            if (evento.ValorTotal <= 0)
            {
                motivoRecusa = "Valor total da compra invalido.";
                pagamento.RecusarPagamento();
            }
            else if (conta is null)
            {
                motivoRecusa = "Conta do usuario nao encontrada.";
                pagamento.RecusarPagamento();
            }
            else if (conta.Saldo < evento.ValorTotal)
            {
                motivoRecusa = "Saldo insuficiente para processar a compra.";
                pagamento.RecusarPagamento();
            }
            else
            {
                conta.Debitar(evento.ValorTotal);
                await contaRepository.DebitarSaldo(conta, evento.ValorTotal);

                pagamento.ConfirmarPagamento();
                aprovado = true;
            }

            await pagamentoRepository.Add(pagamento);

            await messagingPublisher.PublishPaymentProcessedAsync(
                userId: evento.UsuarioId,
                compraId: evento.CompraId,
                emailUsuario: evento.EmailUsuario,
                valorTotal: evento.ValorTotal,
                aprovado: aprovado,
                motivoRecusa: motivoRecusa,
                rastreioId: evento.RastreioId);

            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
            _logger.LogInformation("Resultado de pagamento publicado para a compra {CompraId}.", evento.CompraId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar solicitação de compra. Rejeitando mensagem.");
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
                // Ignora falhas ao fechar o canal.
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
                // Ignora falhas ao fechar a conexão.
            }

            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    private sealed record CompraSolicitadaMensagem(
        int CompraId,
        int UsuarioId,
        IReadOnlyCollection<int> JogosIds,
        decimal ValorTotal,
        DateTime SolicitadaEm,
        string EmailUsuario)
    {
        public string RastreioId { get; init; } = Guid.NewGuid().ToString();
    }
}
