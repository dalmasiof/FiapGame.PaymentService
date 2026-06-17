using System.Threading.Tasks;
using _2_Payment.Application.Interfaces;
using Domain;
using Context;
using Microsoft.EntityFrameworkCore;

namespace _3_Payment.Infrastructure.Repository
{
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
    }
}
