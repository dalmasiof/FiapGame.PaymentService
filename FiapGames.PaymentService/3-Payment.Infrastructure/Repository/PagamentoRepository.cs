using Context;
using Entities;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces;

namespace Repository;
public class PagamentoRepository : IPagamentoRepository
{
    private readonly PaymentContext _context;
        
    public PagamentoRepository(PaymentContext context)
    {
        _context = context;
    }

    public async Task Add(Pagamento pagamento)
    {
        await _context.Pagamentos.AddAsync(pagamento);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Pagamento pagamento)
    {
        _context.Pagamentos.Update(pagamento);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Pagamento>> ObterPorUsuario(int idUsuario)
    {
        return await _context.Pagamentos
            .Include(p => p.Compra)
            .Where(p => p.Compra != null && p.Compra.IdUsuario == idUsuario)
            .ToListAsync();
    }
}
