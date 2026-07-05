using DTOs;

namespace Application.Interfaces;

public interface IContaService
{
    Task<ContaDto> ObterSaldo(int idConta);
    Task<ContaDto> AdicionarSaldo(ContaDto contaDto);
    Task<ContaDto> DebitarSaldo(ContaDto contaDto);
}