namespace Domain
{
    public class Compra
    {
        public int Id { get; private set; }
        public DateTime DataCompra { get; private set; }
        public decimal ValorTotalBruto { get; private set; }
        public decimal ValorTotalLiquido { get; private set; }

        public Compra()
        {
        }

        public Compra(int id)
        {
            Id = id;
            DataCompra = DateTime.Now;
        }
    }
}
