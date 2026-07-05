namespace Entities;

public class Compra
{
    public int Id { get; private set; }
    public int? IdCompraCatalogo { get; private set; }
    public int IdUsuario { get; private set; }
    public DateTime DataCompra { get; private set; }
    public decimal ValorTotalBruto { get; private set; }
    public decimal ValorTotalLiquido { get; private set; }
    public virtual List<CompraJogo> CompraJogos { get; private set; } = new();


    public Compra()
    {
    }

    public Compra(int id, int idUsuario)
    {
        Id = id;
        IdUsuario = idUsuario;
        DataCompra = DateTime.Now;
    }

    public Compra(int id, int idUsuario, decimal valorTotal)
        : this(id, idUsuario)
    {
        if (valorTotal <= 0) throw new ArgumentException("Valor total da compra deve ser maior que zero.", nameof(valorTotal));

        ValorTotalBruto = valorTotal;
        ValorTotalLiquido = valorTotal;
    }

    public Compra(int id, int idUsuario, int idCompraCatalogo, decimal valorTotal)
        : this(id, idUsuario, valorTotal)
    {
        if (idCompraCatalogo <= 0) throw new ArgumentException("ID da compra do catálogo deve ser maior que zero.", nameof(idCompraCatalogo));

        IdCompraCatalogo = idCompraCatalogo;
    }

    public void AdicionarItem(int idJogo, decimal jogoPreco, decimal jogoDesconto)
    {
        CompraJogos.Add(new CompraJogo(idJogo, jogoPreco));

        ValorTotalBruto += jogoPreco;
        ValorTotalLiquido += (jogoPreco - jogoDesconto);
    }
}
