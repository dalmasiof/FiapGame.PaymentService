using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Context;

public sealed class PaymentContextFactory : IDesignTimeDbContextFactory<PaymentContext>
{
    public PaymentContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PaymentContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=fiapgames_payment;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options;

        return new PaymentContext(options);
    }
}
