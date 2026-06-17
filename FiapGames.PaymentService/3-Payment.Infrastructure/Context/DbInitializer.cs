using System.Threading.Tasks;
using Context;

namespace Context
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(PaymentContext context)
        {
            // Placeholder seed: ensure database is created. Real seed logic may be added later.
            await context.Database.EnsureCreatedAsync();
        }
    }
}
