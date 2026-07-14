using _2_Payment.Application.Dtos;
using _2_Payment.Application.Interfaces;
using _2_Payment.Application.Service;

namespace Payment.Application.Test;

public class CompraServiceTests
{
    [Fact]
    public async Task Add_DevePersistirCompraEChamarServicosDependentes()
    {
        var compraRepository = new FakeCompraRepository();
        var contaService = new FakeContaService();
        var bibliotecaService = new FakeBibliotecaService();

        var service = new CompraService(compraRepository, contaService, bibliotecaService);

        await service.Add(new CriarCompraDto(new List<JogoDto>
        {
            new(100, 80m, 10m)
        }, 1));

        Assert.True(compraRepository.AddCalled);
        Assert.True(contaService.DebitarCalled);
        Assert.True(bibliotecaService.AdicionarCalled);
    }

    private sealed class FakeCompraRepository : ICompraRepository
    {
        public bool AddCalled { get; private set; }

        public Task Add(Domain.Compra compra)
        {
            AddCalled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeContaService : IContaService
    {
        public bool DebitarCalled { get; private set; }

        public Task<ContaDto> CriarContaAsync(int idLogin) => Task.FromResult(new ContaDto(idLogin, 0m));

        public Task<ContaDto> AdicionarSaldo(ContaDto contaDto) => Task.FromResult(contaDto);

        public Task<ContaDto> DebitarSaldo(ContaDto contaDto)
        {
            DebitarCalled = true;
            return Task.FromResult(contaDto);
        }

        public Task<ContaDto> ObterSaldo(int idConta) => Task.FromResult(new ContaDto(idConta, 0m));
    }

    private sealed class FakeBibliotecaService : IBibliotecaService
    {
        public bool AdicionarCalled { get; private set; }

        public Task AdicionarJogo(int idUsuario, int idJogo)
        {
            AdicionarCalled = true;
            return Task.CompletedTask;
        }
    }
}
