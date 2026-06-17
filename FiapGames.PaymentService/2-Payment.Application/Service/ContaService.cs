using _2_Payment.Application.Dtos;
using _2_Payment.Application.Interfaces;
using Domain;

namespace _2_Payment.Application.Service
{
    public class ContaService : IContaService
    {
        private readonly IContaRepository _contaRepository;

        public ContaService(IContaRepository contaRepository)
        {
            _contaRepository = contaRepository;
        }

        public async Task<ContaDto> ObterSaldo(int idConta)
        {
            var conta = await _contaRepository.ObterContaPorId(idConta);

            if (conta != null) return new ContaDto(conta.IdConta, conta.Saldo);

            throw new ArgumentException("Conta não encontrada.");
        }

        public async Task<ContaDto> AdicionarSaldo(ContaDto contaDto)
        {
            var conta = await _contaRepository.ObterContaPorId(contaDto.IdConta);

            if (conta == null) throw new ArgumentException("Conta não encontrada.");

            conta.Adicionar(conta.IdConta);
            await _contaRepository.AdicionarSaldo(conta, contaDto.Valor);

            return new ContaDto(conta.IdConta, conta.Saldo);
        }


        public async Task<ContaDto> DebitarSaldo(ContaDto contaDto)
        {
            var conta = await _contaRepository.ObterContaPorId(contaDto.IdConta) ?? throw new ArgumentException("Conta não encontrada.");

            conta.Debitar(contaDto.Valor);
            await _contaRepository.DebitarSaldo(conta, contaDto.Valor);

            return new ContaDto(conta.IdConta, conta.Saldo);
        }
    }
}
