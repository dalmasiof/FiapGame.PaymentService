using System;
using Microsoft.EntityFrameworkCore;
using Domain;
using _2_Payment.Domain.Entities;

namespace Context
{
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
                entity.Property(e => e.DataCompra).IsRequired();
                entity.Property(e => e.ValorTotalBruto).IsRequired();
                entity.Property(e => e.ValorTotalLiquido).IsRequired();
                entity.HasMany<CompraJogo>("CompraJogos")
                      .WithOne()
                      .HasForeignKey("CompraId")
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CompraJogo>(entity =>
            {
                entity.HasKey(e => new { e.CompraId, e.JogoId });
                entity.Property(e => e.PrecoAplicado).IsRequired();
            });

            builder.Entity<Conta>(entity =>
            {
                entity.HasKey(e => e.IdConta);
                entity.Property(e => e.Saldo).IsRequired();
                entity.Property(e => e.DataAtualizacao).IsRequired();
            });

            builder.Entity<Pagamento>(entity =>
            {
                entity.HasKey(e => e.IdPagamento);
                entity.Property(e => e.IdCompra).IsRequired();
                entity.Property(e => e.DataHoraInclusao).IsRequired();
                entity.Property(e => e.Status).IsRequired();
            });
        }
    }
}
