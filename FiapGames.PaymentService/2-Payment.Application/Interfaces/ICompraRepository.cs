using Entities;

namespace Application.Interfaces;

public interface ICompraRepository
{
    Task Add(Compra compra);
    Task<Compra?> ObterPorCompraCatalogoId(int idCompraCatalogo);
}
