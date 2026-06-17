using _2_Payment.Domain.Entities;

namespace Domain
{
    public class Compra
    {
        public int Id { get; private set; }
        public DateTime DataCompra { get; private set; }
        public decimal ValorTotalBruto { get; private set; }
        public decimal ValorTotalLiquido { get; private set; }
        public virtual List<CompraJogo> CompraJogos { get; private set; }


        public Compra()
        {
        }

        public Compra(int id)
        {
            Id = id;
            DataCompra = DateTime.Now;
        }

        public void AdicionarItem(int idJogo, decimal jogoPreco, decimal jogoDesconto)
        {
            CompraJogos = [new CompraJogo(idJogo, jogoPreco)];

            ValorTotalBruto += jogoPreco;
            ValorTotalLiquido += (jogoPreco - jogoDesconto);
        }
    }
}
