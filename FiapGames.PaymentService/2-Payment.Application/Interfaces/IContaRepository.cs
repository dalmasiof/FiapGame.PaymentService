using Domain;

namespace _2_Payment.Application.Interfaces
{
    public interface IContaRepository
    {
        Task<Conta?> ObterContaPorId(int id);
        Task<decimal> ObterSaldo(int idConta);
        Task AdicionarSaldo(Conta conta, decimal valor);
        Task DebitarSaldo(Conta conta, decimal valor);
    }
}
