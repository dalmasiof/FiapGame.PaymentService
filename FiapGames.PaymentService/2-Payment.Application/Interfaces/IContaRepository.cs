using Domain;

namespace _2_Payment.Application.Interfaces
{
    public interface IContaRepository
    {
        Task AdicionarConta(Conta conta);
        Task<Conta?> ObterContaPorId(int id);
        Task<Conta?> ObterContaPorLoginId(int idLogin);
        Task<decimal> ObterSaldo(int idConta);
        Task AdicionarSaldo(Conta conta, decimal valor);
        Task DebitarSaldo(Conta conta, decimal valor);
    }
}
