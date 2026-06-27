using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace Context
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(PaymentContext context)
        {
            await context.Database.EnsureCreatedAsync();

            if (!await context.Contas.AnyAsync())
            {
                var conta = new Conta(150m)
                {
                    IdLogin = 1
                };

                await context.Contas.AddAsync(conta);
            }

            if (!await context.Compras.AnyAsync())
            {
                var compra = new Compra(0, 1);
                compra.AdicionarItem(101, 79.9m, 10m);
                await context.Compras.AddAsync(compra);
                await context.SaveChangesAsync();

                if (!await context.Pagamentos.AnyAsync())
                {
                    var pagamento = new Pagamento(0, compra.Id);
                    await context.Pagamentos.AddAsync(pagamento);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
