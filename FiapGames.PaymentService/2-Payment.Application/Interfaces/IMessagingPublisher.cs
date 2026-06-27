using System.Threading.Tasks;

namespace _2_Payment.Application.Interfaces
{
    public interface IMessagingPublisher
    {
        Task PublishAddGameAsync(int userId, int gameId);
    }
}
