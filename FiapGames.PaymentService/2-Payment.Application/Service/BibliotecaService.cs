using System.Threading.Tasks;
using _2_Payment.Application.Interfaces;

namespace _2_Payment.Application.Service
{
    public class BibliotecaService : IBibliotecaService
    {
        private readonly IMessagingPublisher _publisher;

        public BibliotecaService(IMessagingPublisher publisher)
        {
            _publisher = publisher;
        }

        public async Task AdicionarJogo(int idUsuario, int idJogo)
        {
            // Encapsula a chamada de mensageria em uma interface.
            // Implementação real de mensageria fica na infraestrutura.
            await _publisher.PublishAddGameAsync(idUsuario, idJogo);
        }
    }
}
