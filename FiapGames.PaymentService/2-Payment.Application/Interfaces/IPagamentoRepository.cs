using Domain;

namespace _2_Payment.Application.Interfaces
{
    public interface IPagamentoRepository
    {
        Task Add(Pagamento pagamento);
        Task Update(Pagamento pagamento);
    }
}
