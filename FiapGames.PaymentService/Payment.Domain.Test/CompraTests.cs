using Domain;

namespace Payment.Domain.Test;

public class CompraTests
{
    [Fact]
    public void AdicionarItem_DeveSomarValoresBrutoELiquido()
    {
        var compra = new Compra(1, 10);

        compra.AdicionarItem(100, 80m, 10m);
        compra.AdicionarItem(101, 50m, 5m);

        Assert.Equal(130m, compra.ValorTotalBruto);
        Assert.Equal(115m, compra.ValorTotalLiquido);
    }

    [Fact]
    public void Debitar_DeveLancarExcecaoQuandoSaldoInsuficiente()
    {
        var conta = new Conta(50m);

        var ex = Assert.Throws<ArgumentException>(() => conta.Debitar(60m));

        Assert.Contains("Saldo insuficiente", ex.Message);
    }
}
