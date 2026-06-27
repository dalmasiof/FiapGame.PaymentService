using _2_Payment.Application.Dtos;

namespace _2_Payment.Application.Interfaces
{
    public interface ICompraService
    {
        Task Add(CriarCompraDto compra);

    }
}
