using Context;
using Application.Interfaces;
using Messaging;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Repository;
using Service;

var builder = WebApplication.CreateBuilder(args);

var rabbitHost = builder.Configuration["RabbitMq:HostName"];
var rabbitPort = int.Parse(builder.Configuration["RabbitMq:Port"] ?? "5672");
var rabbitUser = builder.Configuration["RabbitMq:UserName"];
var rabbitPass = builder.Configuration["RabbitMq:Password"];

builder.Services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory
{
    HostName = rabbitHost,
    Port = rabbitPort,
    UserName = rabbitUser,
    Password = rabbitPass,
    AutomaticRecoveryEnabled = true,
    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
});

// Add services to the container.

builder.Services.AddControllers();

// Dependency injection registrations
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IContaService, ContaService>();
builder.Services.AddScoped<IBibliotecaService, BibliotecaService>();
builder.Services.AddScoped<IPagamentoService, PagamentoService>();

builder.Services.AddScoped<ICompraRepository, CompraRepository>();
builder.Services.AddScoped<IContaRepository, ContaRepository>();
builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();

builder.Services.AddScoped<IMessagePublisher, RabbitMqPublisher>();
builder.Services.AddHostedService<CompraSolicitadaConsumer>();

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
            await context.Database.MigrateAsync();
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
