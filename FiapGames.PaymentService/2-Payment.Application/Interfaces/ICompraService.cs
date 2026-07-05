using DTOs;

namespace Application.Interfaces;

public interface ICompraService
{
    Task Add(CriarCompraDto compra);

}