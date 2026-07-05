using Entities;
using IntegrationEvents;

namespace Application.Interfaces;

public interface IPagamentoService
{
    Task Add(Pagamento pagamento);
    Task Update(Pagamento pagamento);
    Task<IEnumerable<Pagamento>> ObterPorUsuario(int idUsuario);
    Task ProcessarCompraSolicitadaAsync(CompraSolicitadaIntegrationEvent evento);
}
