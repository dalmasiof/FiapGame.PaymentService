using Entities;

namespace Domain.Interfaces;
public interface ICompraRepository
{
    Task Add(Compra compra);
}