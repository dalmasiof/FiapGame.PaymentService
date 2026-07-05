using Application.Interfaces;

namespace Service;

public class BibliotecaService : IBibliotecaService
{
    private readonly IMessagePublisher _publisher;

    public BibliotecaService(IMessagePublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task AdicionarJogo(int idUsuario, int idJogo)
    {
        // 1. Criamos um objeto anônimo ou um Record para representar a mensagem
        // O ideal é criar um record separado chamado "JogoAdicionadoIntegrationEvent"
        var evento = new
        {
            UsuarioId = idUsuario,
            JogoId = idJogo,
            DataAdicao = DateTime.UtcNow,
            RastreioId = Guid.NewGuid().ToString() // O nosso ID de segurança
        };

        // 2. Chamamos o MÉTODo da interface (PublishAsync), e não o nome da classe!
        // Passamos a mensagem, a routing key e o id de rastreio
        await _publisher.PublishAsync(
            message: evento,
            routingKey: "biblioteca.jogo.adicionado", // A etiqueta desta mensagem
            correlationId: evento.RastreioId
        );
    }
}