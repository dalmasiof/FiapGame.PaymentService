using Entities;

namespace Application.Interfaces;

public interface IContaRepository
{
    Task<Conta?> ObterContaPorId(int id);
    Task<Conta?> ObterContaPorUsuarioId(int idUsuario);
    Task<decimal> ObterSaldo(int idConta);
    Task AdicionarSaldo(Conta conta, decimal valor);
    Task DebitarSaldo(Conta conta, decimal valor);
}
