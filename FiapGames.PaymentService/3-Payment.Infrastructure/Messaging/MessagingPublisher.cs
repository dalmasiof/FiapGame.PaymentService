using System;
using System.Threading.Tasks;
using _2_Payment.Application.Interfaces;

namespace _3_Payment.Infrastructure.Messaging
{
    public class MessagingPublisher : IMessagingPublisher
    {
        public Task PublishAddGameAsync(int userId, int gameId)
        {
            // Mensageria ainda não implementada — stub intencional.
            throw new NotImplementedException("Mensageria não implementada. Implementar transporte (RabbitMQ/Kafka/etc.) aqui.");
        }
    }
}
