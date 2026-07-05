using Microsoft.EntityFrameworkCore;
using Entities;

namespace Context;

public class PaymentContext : DbContext
{
    public PaymentContext(DbContextOptions<PaymentContext> options) : base(options)
    {
    }

    public DbSet<Compra> Compras { get; set; }
    public DbSet<CompraJogo> CompraJogos { get; set; }
    public DbSet<Conta> Contas { get; set; }
    public DbSet<Pagamento> Pagamentos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IdCompraCatalogo)
                  .IsUnique()
                  .HasFilter("[IdCompraCatalogo] IS NOT NULL");

            entity.Property(e => e.IdCompraCatalogo);
            entity.Property(e => e.IdUsuario).IsRequired();
            entity.Property(e => e.DataCompra).IsRequired();

            // Added precision to prevent truncation warnings
            entity.Property(e => e.ValorTotalBruto).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.ValorTotalLiquido).IsRequired().HasPrecision(18, 2);

            entity.HasMany<CompraJogo>("CompraJogos")
                  .WithOne()
                  .HasForeignKey("CompraId")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CompraJogo>(entity =>
        {
            entity.HasKey(e => new { e.CompraId, e.JogoId });

            // Added precision to prevent truncation warnings
            entity.Property(e => e.PrecoAplicado).IsRequired().HasPrecision(18, 2);
        });

        builder.Entity<Conta>(entity =>
        {
            entity.HasKey(e => e.IdConta);

            // Added precision to prevent truncation warnings
            entity.Property(e => e.Saldo).IsRequired().HasPrecision(18, 2);

            entity.Property(e => e.DataAtualizacao).IsRequired();
        });

        builder.Entity<Pagamento>(entity =>
        {
            entity.HasKey(e => e.IdPagamento);
            entity.Property(e => e.IdCompra).IsRequired();
            entity.Property(e => e.DataHoraInclusao).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.HasOne(e => e.Compra)
                  .WithMany()
                  .HasForeignKey(e => e.IdCompra)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
