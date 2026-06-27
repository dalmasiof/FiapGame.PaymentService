using _2_Payment.Application.Interfaces;
using _2_Payment.Application.Service;
using _3_Payment.Infrastructure.Messaging;
using _3_Payment.Infrastructure.Repository;
using Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();


// Dependency injection registrations
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IContaService, ContaService>();
builder.Services.AddScoped<IBibliotecaService, BibliotecaService>();

builder.Services.AddScoped<ICompraRepository, CompraRepository>();
builder.Services.AddScoped<IContaRepository, ContaRepository>();
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();
builder.Services.AddScoped<IPagamentoService, PagamentoService>();

// Mensageria stub (a implementação real deve ser adicionada posteriormente)
builder.Services.AddScoped<IMessagingPublisher, MessagingPublisher>();

var connectionString = builder.Configuration.GetConnectionString("FIAPGamesConnection");
builder.Services.AddDbContext<PaymentContext>(opts =>
    opts
        .UseLazyLoadingProxies()
        .UseSqlServer(
            connectionString,
            sqlServerOptionsAction: sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await InitializeDatabaseAsync(app);

app.Run();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    var logger = app.Logger;
    const int maxAttempts = 20;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PaymentContext>();
            await context.Database.EnsureCreatedAsync();
            await DbInitializer.SeedAsync(context);
            logger.LogInformation("Database initialized successfully.");
            return;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            logger.LogWarning(ex,
                "Database initialization failed (attempt {Attempt}/{MaxAttempts}). Retrying in 5 seconds...",
                attempt,
                maxAttempts);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
