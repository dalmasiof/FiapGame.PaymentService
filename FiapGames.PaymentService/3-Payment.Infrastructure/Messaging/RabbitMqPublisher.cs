using Application.Interfaces;
using RabbitMQ.Client;
using System.Text.Json;

namespace Messaging;

public class RabbitMqPublisher : IMessagePublisher, IAsyncDisposable
{
    private const string PaymentExchangeName = "pagamento.exchange";

    private readonly IConnectionFactory _connectionFactory;
    private readonly HashSet<string> _declaredExchanges = new();
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqPublisher(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private async Task ConnectAsync(string exchangeName)
    {
        if (_channel is null)
        {
            _connection = await _connectionFactory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
        }

        if (_declaredExchanges.Add(exchangeName))
        {
            await _channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);
        }
    }

    public Task PublishAsync<T>(T message, string routingKey, string correlationId) where T : class
    {
        return PublishAsync(message, PaymentExchangeName, routingKey, correlationId);
    }

    public async Task PublishAsync<T>(T message, string exchange, string routingKey, string correlationId) where T : class
    {
        await ConnectAsync(exchange);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            CorrelationId = correlationId
        };

        await _channel!.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
    }
}
