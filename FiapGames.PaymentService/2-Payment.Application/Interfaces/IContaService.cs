using _2_Payment.Application.Dtos;
using Domain;

namespace _2_Payment.Application.Interfaces
{
    public interface IContaService
    {
        Task<ContaDto> CriarContaAsync(int idLogin);
        Task<ContaDto> ObterSaldo(int idConta);
        Task<ContaDto> AdicionarSaldo(ContaDto contaDto);
        Task<ContaDto> DebitarSaldo(ContaDto contaDto);
    }
}
