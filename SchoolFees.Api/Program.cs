using Microsoft.EntityFrameworkCore;
using SchoolFees.Application.Payments;
using SchoolFees.Infrastructure.Data;
using SchoolFees.Infrastructure.Repositories;
using SchoolFees.Api.Services;
using SchoolFees.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure Entity Framework and repositories
builder.Services.AddDbContext<FeesDbContext>(o => o.UseSqlite("DataSource=:memory:"));
builder.Services.AddScoped<IPaymentRepository, EfPaymentRepository>();
builder.Services.AddScoped<IIdempotencyStore, EfIdempotencyStore>();

// Configure metrics and application services
builder.Services.AddSingleton<MetricsService>();
builder.Services.AddSingleton<IMetricsService>(provider => provider.GetRequiredService<MetricsService>());
builder.Services.AddScoped<IMetricsTracker>(provider => provider.GetRequiredService<MetricsService>());
builder.Services.AddScoped<CreatePaymentHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FeesDbContext>();
    await context.Database.OpenConnectionAsync();
    await context.Database.EnsureCreatedAsync();
}

// Payment endpoint
app.MapPost("/fees/{studentId:guid}/payments", 
    async (Guid studentId, PaymentRequest req, HttpRequest http, CreatePaymentHandler handler, CancellationToken ct) =>
    {
        var key = http.Headers["Idempotency-Key"].FirstOrDefault();
        var cmd = new CreatePaymentCommand(studentId, req.Amount, req.Method, key);
        var result = await handler.Handle(cmd, ct);
        return Results.Created($"/fees/{studentId}/payments/{result.Id}", result);
    });

app.Run();

record PaymentRequest(decimal Amount, string Method);

// Make Program class accessible for integration tests
public partial class Program { }
