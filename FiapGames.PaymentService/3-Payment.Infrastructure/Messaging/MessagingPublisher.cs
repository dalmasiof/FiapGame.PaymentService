using System.Text;
using System.Text.Json;
using _2_Payment.Application.Interfaces;
using RabbitMQ.Client;

namespace _3_Payment.Infrastructure.Messaging
{
    public class MessagingPublisher : IMessagingPublisher, IAsyncDisposable
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection? _connection;
        private IChannel? _channel;
        private IChannel? _notificationChannel;

        private const string PaymentExchangeName = "pagamento.exchange";
        private const string NotificationExchangeName = "notificacao.exchange";
        private const string RoutingKeyApproved = "pagamento.aprovado";
        private const string RoutingKeyRejected = "pagamento.recusado";
        private const string NotificationRoutingKey = "pagamento.notificacao";

        public MessagingPublisher(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public Task PublishAddGameAsync(int userId, int gameId)
        {
            return PublishPaymentProcessedAsync(
                userId: userId,
                compraId: gameId,
                emailUsuario: string.Empty,
                valorTotal: 0m,
                aprovado: true,
                motivoRecusa: null);
        }

        public async Task PublishPaymentProcessedAsync(
            int userId,
            int compraId,
            string emailUsuario,
            decimal valorTotal,
            bool aprovado,
            string? motivoRecusa,
            string? rastreioId = null)
        {
            await EnsureConnectedAsync();

            var status = aprovado ? "Aprovado" : "Reprovado";
            var routingKey = aprovado ? RoutingKeyApproved : RoutingKeyRejected;
            var correlationId = rastreioId ?? Guid.NewGuid().ToString();

            var payload = JsonSerializer.Serialize(new
            {
                CompraId = compraId,
                UsuarioId = userId,
                Aprovado = aprovado,
                ValorTotal = valorTotal,
                Status = status,
                ProcessadoEm = DateTime.UtcNow,
                RastreioId = correlationId,
                MotivoRecusa = motivoRecusa
            });

            var body = Encoding.UTF8.GetBytes(payload);
            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                CorrelationId = correlationId
            };

            await _channel!.BasicPublishAsync(
                exchange: PaymentExchangeName,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body);

            if (aprovado && !string.IsNullOrWhiteSpace(emailUsuario))
            {
                await PublishNotificationAsync(compraId, emailUsuario, valorTotal, correlationId);
            }
        }

        private async Task EnsureConnectedAsync()
        {
            if (_channel is { IsOpen: true }) return;

            _connection = await _connectionFactory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            _notificationChannel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(exchange: PaymentExchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false);
            await _notificationChannel.ExchangeDeclareAsync(exchange: NotificationExchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false);
        }

        private async Task PublishNotificationAsync(int compraId, string destinatario, decimal valorTotal, string correlationId)
        {
            var correlacaoId = Guid.TryParse(correlationId, out var parsedCorrelationId)
                ? parsedCorrelationId
                : Guid.NewGuid();

            var notificationPayload = JsonSerializer.Serialize(new
            {
                correlacaoId,
                destinatario,
                assunto = "Compra aprovada no FiapGames",
                corpoMensagem = $"Sua compra {compraId} foi aprovada no valor de {valorTotal:F2}.",
                dominioOrigem = "Pagamento"
            });

            var notificationBody = Encoding.UTF8.GetBytes(notificationPayload);
            var notificationProperties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                CorrelationId = correlationId
            };

            await _notificationChannel!.BasicPublishAsync(
                exchange: NotificationExchangeName,
                routingKey: NotificationRoutingKey,
                mandatory: true,
                basicProperties: notificationProperties,
                body: notificationBody);
        }

        public async ValueTask DisposeAsync()
        {
            if (_notificationChannel is not null) await _notificationChannel.DisposeAsync();
            if (_channel is not null) await _channel.DisposeAsync();
            if (_connection is not null) await _connection.DisposeAsync();
        }
    }
}
