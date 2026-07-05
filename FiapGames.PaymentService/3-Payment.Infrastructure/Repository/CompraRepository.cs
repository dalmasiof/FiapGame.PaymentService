using Application.Interfaces;
using Context;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public class CompraRepository : ICompraRepository
{
    private readonly PaymentContext _context;

    public CompraRepository(PaymentContext context)
    {
        _context = context;
    }

    public async Task Add(Compra compra)
    {
        await _context.Compras.AddAsync(compra);
        await _context.SaveChangesAsync();
    }

    public async Task<Compra?> ObterPorCompraCatalogoId(int idCompraCatalogo)
    {
        return await _context.Compras.FirstOrDefaultAsync(c => c.IdCompraCatalogo == idCompraCatalogo);
    }
}
