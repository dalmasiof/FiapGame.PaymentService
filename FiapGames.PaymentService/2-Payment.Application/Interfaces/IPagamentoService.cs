using Domain;

namespace _2_Payment.Application.Interfaces
{
    public interface IPagamentoService
    {
        Task Add(Pagamento pagamento);
        Task Update(Pagamento pagamento);
    }
}
