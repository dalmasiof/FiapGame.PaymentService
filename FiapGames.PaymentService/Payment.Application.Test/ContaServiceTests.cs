using _2_Payment.Application.Dtos;
using _2_Payment.Application.Interfaces;
using _2_Payment.Application.Service;
using Domain;

namespace Payment.Application.Test;

public class ContaServiceTests
{
    [Fact]
    public async Task CriarContaAsync_DeveCriarNovaConta_QuandoLoginNaoPossuiConta()
    {
        var repository = new FakeContaRepository();
        var service = new ContaService(repository);

        var conta = await service.CriarContaAsync(10);

        Assert.Equal(1, repository.AdicionarContaCalls);
        Assert.Equal(10, repository.StoredConta?.IdLogin);
        Assert.Equal(repository.StoredConta?.IdConta, conta.IdConta);
        Assert.Equal(0m, conta.Valor);
    }

    [Fact]
    public async Task CriarContaAsync_DeveReutilizarContaExistente_QuandoLoginJaPossuiConta()
    {
        var repository = new FakeContaRepository
        {
            ContaPorLogin = new Conta(25m)
            {
                IdConta = 7,
                IdLogin = 10
            }
        };
        var service = new ContaService(repository);

        var conta = await service.CriarContaAsync(10);

        Assert.Equal(0, repository.AdicionarContaCalls);
        Assert.Equal(7, conta.IdConta);
        Assert.Equal(25m, conta.Valor);
    }

    private sealed class FakeContaRepository : IContaRepository
    {
        public int AdicionarContaCalls { get; private set; }
        public Conta? ContaPorLogin { get; set; }
        public Conta? StoredConta { get; private set; }

        public Task AdicionarConta(Conta conta)
        {
            AdicionarContaCalls++;
            conta.IdConta = 99;
            StoredConta = conta;
            ContaPorLogin = conta;
            return Task.CompletedTask;
        }

        public Task AdicionarSaldo(Conta conta, decimal valor) => Task.CompletedTask;

        public Task DebitarSaldo(Conta conta, decimal valor) => Task.CompletedTask;

        public Task<Conta?> ObterContaPorId(int id)
        {
            var conta = StoredConta is not null && StoredConta.IdConta == id ? StoredConta : null;
            return Task.FromResult(conta);
        }

        public Task<Conta?> ObterContaPorLoginId(int idLogin)
        {
            var conta = ContaPorLogin is not null && ContaPorLogin.IdLogin == idLogin ? ContaPorLogin : null;
            return Task.FromResult(conta);
        }

        public Task<decimal> ObterSaldo(int idConta)
        {
            var saldo = StoredConta is not null && StoredConta.IdConta == idConta ? StoredConta.Saldo : 0m;
            return Task.FromResult(saldo);
        }
    }
}