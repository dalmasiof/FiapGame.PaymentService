using System.Threading.Tasks;
using _2_Payment.Application.Interfaces;
using Domain;
using Context;
using Microsoft.EntityFrameworkCore;

namespace _3_Payment.Infrastructure.Repository
{
    public class ContaRepository : IContaRepository
    {
        private readonly PaymentContext _context;

        public ContaRepository(PaymentContext context)
        {
            _context = context;
        }

        public async Task AdicionarSaldo(Conta conta, decimal valor)
        {
            // conta já alterada pelo domínio
            _context.Contas.Update(conta);
            await _context.SaveChangesAsync();
        }

        public async Task DebitarSaldo(Conta conta, decimal valor)
        {
            _context.Contas.Update(conta);
            await _context.SaveChangesAsync();
        }

        public async Task<Conta?> ObterContaPorId(int id)
        {
            return await _context.Contas.FirstOrDefaultAsync(c => c.IdConta == id);
        }

        public async Task<Conta?> ObterContaPorLoginId(int idLogin)
        {
            return await _context.Contas.FirstOrDefaultAsync(c => c.IdLogin == idLogin);
        }

        public async Task<decimal> ObterSaldo(int idConta)
        {
            var conta = await ObterContaPorId(idConta);
            return conta?.Saldo ?? 0m;
        }
    }
}
