using _2_Payment.Application.Interfaces;
using _2_Payment.Application.Service;
using _3_Payment.Infrastructure.Repository;
using _3_Payment.Infrastructure.Messaging;
using Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Dependency injection registrations
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IContaService, ContaService>();
builder.Services.AddScoped<IBibliotecaService, BibliotecaService>();

builder.Services.AddScoped<ICompraRepository, CompraRepository>();
builder.Services.AddScoped<IContaRepository, ContaRepository>();
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();

// Mensageria stub (a implementação real deve ser adicionada posteriormente)
builder.Services.AddScoped<IMessagingPublisher, MessagingPublisher>();

var connectionString = builder.Configuration.GetConnectionString("FIAPGamesConnection");
builder.Services.AddDbContext<PaymentContext>(opts =>
    opts
    .UseLazyLoadingProxies()
    .UseSqlServer(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PaymentContext>();
    await DbInitializer.SeedAsync(context);
}

app.Run();
