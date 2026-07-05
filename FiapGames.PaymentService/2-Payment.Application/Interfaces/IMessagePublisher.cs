namespace Application.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, string routingKey, string correlationId) where T : class;
    Task PublishAsync<T>(T message, string exchange, string routingKey, string correlationId) where T : class;
}
