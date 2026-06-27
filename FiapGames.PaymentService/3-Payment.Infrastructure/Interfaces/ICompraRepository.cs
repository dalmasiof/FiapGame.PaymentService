using Domain;

namespace _3_Payment.Infrastructure.Interfaces
{
    public interface ICompraRepository
    {
        Task Add(Compra compra);

    }
}
