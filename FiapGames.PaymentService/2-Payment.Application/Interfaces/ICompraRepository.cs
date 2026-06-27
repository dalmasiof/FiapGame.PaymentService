using Domain;

namespace _2_Payment.Application.Interfaces
{
    public interface ICompraRepository
    {
        Task Add(Compra compra);
    }
}
