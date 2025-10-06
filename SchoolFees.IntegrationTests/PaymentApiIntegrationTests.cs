using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolFees.Application.Payments;
using SchoolFees.Infrastructure.Data;

namespace SchoolFees.IntegrationTests;

[TestFixture]
public class PaymentApiIntegrationTests : IDisposable
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private IServiceScope _scope;
    private FeesDbContext _context;

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<FeesDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add a test database context using in-memory database with a unique connection string
                    var connectionString = $"DataSource=TestDb_{Guid.NewGuid()};Mode=Memory;Cache=Shared";
                    services.AddDbContext<FeesDbContext>(options =>
                    {
                        options.UseSqlite(connectionString);
                    });
                });
                
                builder.UseEnvironment("Testing");
            });
        
        _client = _factory.CreateClient();
        
        // Create a scope and keep the database connection alive for the entire test
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<FeesDbContext>();
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Database.CloseConnection();
        _context?.Dispose();
        _scope?.Dispose();
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task PostPayment_WithValidData_ShouldReturn201Created()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var paymentRequest = new { Amount = 100.50m, Method = "Credit Card" };
        var json = JsonSerializer.Serialize(paymentRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        content.Headers.Add("Idempotency-Key", "unique-test-key-1");

        // Act
        var response = await _client.PostAsync($"/fees/{studentId}/payments", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/fees/{studentId}/payments/");

        var responseBody = await response.Content.ReadAsStringAsync();
        var paymentDto = JsonSerializer.Deserialize<PaymentDto>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        paymentDto.Should().NotBeNull();
        paymentDto!.Id.Should().NotBe(Guid.Empty);
        paymentDto.StudentId.Should().Be(studentId);
        paymentDto.Amount.Should().Be(100.50m);
        paymentDto.Method.Should().Be("Credit Card");
        paymentDto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Test]
    public async Task PostPayment_WithSameIdempotencyKey_ShouldReturnSamePaymentId()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var paymentRequest = new { Amount = 150.75m, Method = "Bank Transfer" };
        var json = JsonSerializer.Serialize(paymentRequest);
        var idempotencyKey = "duplicate-test-key";

        // Act - First request
        var content1 = new StringContent(json, Encoding.UTF8, "application/json");
        content1.Headers.Add("Idempotency-Key", idempotencyKey);
        var response1 = await _client.PostAsync($"/fees/{studentId}/payments", content1);

        // Act - Second request with same idempotency key
        var content2 = new StringContent(json, Encoding.UTF8, "application/json");
        content2.Headers.Add("Idempotency-Key", idempotencyKey);
        var response2 = await _client.PostAsync($"/fees/{studentId}/payments", content2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseBody1 = await response1.Content.ReadAsStringAsync();
        var responseBody2 = await response2.Content.ReadAsStringAsync();

        var payment1 = JsonSerializer.Deserialize<PaymentDto>(responseBody1, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var payment2 = JsonSerializer.Deserialize<PaymentDto>(responseBody2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        payment1.Should().NotBeNull();
        payment2.Should().NotBeNull();
        
        // Both responses should return the same payment ID (idempotency)
        payment2!.Id.Should().Be(payment1!.Id);
        payment2.StudentId.Should().Be(payment1.StudentId);
        payment2.Amount.Should().Be(payment1.Amount);
        payment2.Method.Should().Be(payment1.Method);
        payment2.CreatedAt.Should().Be(payment1.CreatedAt);

        // Location headers should also be the same
        response1.Headers.Location.Should().Be(response2.Headers.Location);
    }

    [Test]
    public async Task PostPayment_WithDifferentIdempotencyKeys_ShouldCreateDifferentPayments()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var paymentRequest = new { Amount = 200.00m, Method = "Debit Card" };
        var json = JsonSerializer.Serialize(paymentRequest);

        // Act - First request
        var content1 = new StringContent(json, Encoding.UTF8, "application/json");
        content1.Headers.Add("Idempotency-Key", "key-1");
        var response1 = await _client.PostAsync($"/fees/{studentId}/payments", content1);

        // Act - Second request with different idempotency key
        var content2 = new StringContent(json, Encoding.UTF8, "application/json");
        content2.Headers.Add("Idempotency-Key", "key-2");
        var response2 = await _client.PostAsync($"/fees/{studentId}/payments", content2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseBody1 = await response1.Content.ReadAsStringAsync();
        var responseBody2 = await response2.Content.ReadAsStringAsync();

        var payment1 = JsonSerializer.Deserialize<PaymentDto>(responseBody1, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var payment2 = JsonSerializer.Deserialize<PaymentDto>(responseBody2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        payment1.Should().NotBeNull();
        payment2.Should().NotBeNull();
        
        // Different idempotency keys should create different payments
        payment2!.Id.Should().NotBe(payment1!.Id);
        payment2.StudentId.Should().Be(payment1.StudentId); // Same student
        payment2.Amount.Should().Be(payment1.Amount); // Same amount
        payment2.Method.Should().Be(payment1.Method); // Same method
        
        // But different IDs and potentially different timestamps
        response1.Headers.Location.Should().NotBe(response2.Headers.Location);
    }

    [Test]
    public async Task PostPayment_WithoutIdempotencyKey_ShouldCreateNewPaymentsEachTime()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var paymentRequest = new { Amount = 75.25m, Method = "Cash" };
        var json = JsonSerializer.Serialize(paymentRequest);

        // Act - First request without idempotency key
        var content1 = new StringContent(json, Encoding.UTF8, "application/json");
        var response1 = await _client.PostAsync($"/fees/{studentId}/payments", content1);

        // Act - Second request without idempotency key
        var content2 = new StringContent(json, Encoding.UTF8, "application/json");
        var response2 = await _client.PostAsync($"/fees/{studentId}/payments", content2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseBody1 = await response1.Content.ReadAsStringAsync();
        var responseBody2 = await response2.Content.ReadAsStringAsync();

        var payment1 = JsonSerializer.Deserialize<PaymentDto>(responseBody1, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var payment2 = JsonSerializer.Deserialize<PaymentDto>(responseBody2, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        payment1.Should().NotBeNull();
        payment2.Should().NotBeNull();
        
        // Without idempotency keys, should create different payments
        payment2!.Id.Should().NotBe(payment1!.Id);
        response1.Headers.Location.Should().NotBe(response2.Headers.Location);
    }

    [Test]
    public async Task PostPayment_WithInvalidStudentId_ShouldReturn404NotFound()
    {
        // Arrange
        var invalidStudentId = "not-a-guid";
        var paymentRequest = new { Amount = 100.00m, Method = "Credit Card" };
        var json = JsonSerializer.Serialize(paymentRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/fees/{invalidStudentId}/payments", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _scope?.Dispose();
        _client?.Dispose();
        _factory?.Dispose();
    }
}
