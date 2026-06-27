using Domain;

namespace _3_Payment.Infrastructure.Interfaces
{
    public interface IContaRepository
    {
        Task<Conta?> ObterContaPorId(int id);
        Task<decimal> ObterSaldo(int idConta);
        Task AdicionarSaldo(Conta conta, decimal valor);
        Task DebitarSaldo(Conta conta, decimal valor);
    }
}
