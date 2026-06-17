using _2_Payment.Application.Dtos;
using _2_Payment.Application.Interfaces;
using Domain;

namespace _2_Payment.Application.Service
{
    public class CompraService : ICompraService
    {
        private readonly ICompraRepository _compraRepository;
        private readonly IContaService _contaService;
        private readonly IBibliotecaService _bibliotecaService;

        public CompraService(ICompraRepository compraRepository, IContaService contaService, IBibliotecaService bibliotecaService)
        {
            _compraRepository = compraRepository;
            _contaService = contaService;
            _bibliotecaService = bibliotecaService;
        }

        public async Task Add(CriarCompraDto compraDto)
        {
            var compra = new Compra(0);

            foreach (var jogo in compraDto.Jogos)
            {
                compra.AdicionarItem(jogo.IdJogo, jogo.PrecoJogo, jogo.DescontoJogo);
            }

            // persiste a compra
            await _compraRepository.Add(compra);

            // debita saldo do usuário
            await _contaService.DebitarSaldo(new ContaDto(compraDto.IdUsuario, compra.ValorTotalLiquido));

            // adiciona jogos na biblioteca do usuário (encapsulado na aplicação)
            foreach (var jogo in compraDto.Jogos)
            {
                await _bibliotecaService.AdicionarJogo(compraDto.IdUsuario, jogo.IdJogo);
            }
        }
    }
}
