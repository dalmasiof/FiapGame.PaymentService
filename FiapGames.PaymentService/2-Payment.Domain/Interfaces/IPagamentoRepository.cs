using Entities;

namespace Domain.Interfaces;
public interface IPagamentoRepository
{
    Task Add(Pagamento pagamento);
    Task Update(Pagamento pagamento);
}