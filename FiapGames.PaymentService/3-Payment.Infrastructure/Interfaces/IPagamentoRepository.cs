using Domain;

namespace _3_Payment.Infrastructure.Interfaces
{
    public interface IPagamentoRepository
    {
        Task Add(Pagamento pagamento);
        Task Update(Pagamento pagamento);
    }
}
