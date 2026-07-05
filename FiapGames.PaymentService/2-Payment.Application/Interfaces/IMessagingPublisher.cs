using System.Threading.Tasks;

namespace _2_Payment.Application.Interfaces
{
    public interface IMessagingPublisher
    {
        Task PublishAddGameAsync(int userId, int gameId);

        Task PublishPaymentProcessedAsync(
            int userId,
            int compraId,
            string emailUsuario,
            decimal valorTotal,
            bool aprovado,
            string? motivoRecusa,
            string? rastreioId = null);
    }
}
