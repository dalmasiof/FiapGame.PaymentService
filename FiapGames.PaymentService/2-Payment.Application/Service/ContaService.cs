using _2_Payment.Application.Dtos;
using _2_Payment.Application.Interfaces;
using Domain;
using System.Linq;

namespace _2_Payment.Application.Service
{
    public class ContaService : IContaService
    {
        private readonly IContaRepository _contaRepository;

        public ContaService(IContaRepository contaRepository)
        {
            _contaRepository = contaRepository;
        }

        public async Task<ContaDto> CriarContaAsync(int idLogin)
        {
            var contaExistente = await _contaRepository.ObterContaPorLoginId(idLogin);
            if (contaExistente is not null)
            {
                return new ContaDto(contaExistente.IdConta, contaExistente.Saldo);
            }

            var conta = new Conta(0)
            {
                IdLogin = idLogin
            };

            await _contaRepository.AdicionarConta(conta);

            return new ContaDto(conta.IdConta, conta.Saldo);
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

            conta.Adicionar(contaDto.Valor);
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

        public async Task<IEnumerable<ContaDetalhesDto>> ListarContasAsync()
        {
            var contas = await _contaRepository.ListarContasAsync();

            return contas.Select(conta => new ContaDetalhesDto(
                conta.IdConta,
                conta.IdLogin,
                conta.Saldo,
                conta.DataAtualizacao));
        }
    }
}
