using System.Threading.Tasks;
using _2_Payment.Application.Interfaces;
using Domain;
using Context;
using Microsoft.EntityFrameworkCore;

namespace _3_Payment.Infrastructure.Repository
{
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
    }
}
