using FluentAssertions;
using SchoolFees.Application.Payments;
using SchoolFees.Domain;

namespace SchoolFees.UnitTests.Application;

[TestFixture]
public class CreatePaymentHandlerTests
{
    private CreatePaymentHandler _handler;
    private InMemoryPaymentRepository _paymentRepository;
    private InMemoryIdempotencyStore _idempotencyStore;
    private InMemoryMetricsTracker _metricsTracker;

    [SetUp]
    public void SetUp()
    {
        _paymentRepository = new InMemoryPaymentRepository();
        _idempotencyStore = new InMemoryIdempotencyStore();
        _metricsTracker = new InMemoryMetricsTracker();
        _handler = new CreatePaymentHandler(_paymentRepository, _idempotencyStore, _metricsTracker);
    }

    [Test]
    public async Task Handle_WithNewIdempotencyKey_ShouldCreateNewPayment()
    {
        // Arrange
        var command = new CreatePaymentCommand(
            Guid.NewGuid(), 
            100.50m, 
            "Credit Card", 
            "unique-key-1");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.StudentId.Should().Be(command.StudentId);
        result.Amount.Should().Be(command.Amount);
        result.Method.Should().Be(command.Method);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public async Task Handle_WithSameIdempotencyKey_ShouldReturnSameResult()
    {
        // Arrange
        var command = new CreatePaymentCommand(
            Guid.NewGuid(), 
            100.50m, 
            "Credit Card", 
            "duplicate-key");

        // Act
        var firstResult = await _handler.Handle(command, CancellationToken.None);
        var secondResult = await _handler.Handle(command, CancellationToken.None);

        // Assert
        firstResult.Should().NotBeNull();
        secondResult.Should().NotBeNull();
        secondResult.Id.Should().Be(firstResult.Id);
        secondResult.StudentId.Should().Be(firstResult.StudentId);
        secondResult.Amount.Should().Be(firstResult.Amount);
        secondResult.Method.Should().Be(firstResult.Method);
        secondResult.CreatedAt.Should().Be(firstResult.CreatedAt);
    }

    [Test]
    public async Task Handle_WithSameIdempotencyKey_ShouldNotCreateDuplicatePayment()
    {
        // Arrange
        var command = new CreatePaymentCommand(
            Guid.NewGuid(), 
            100.50m, 
            "Credit Card", 
            "duplicate-key");

        // Act
        await _handler.Handle(command, CancellationToken.None);
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var allPayments = await _paymentRepository.GetAllAsync();
        allPayments.Should().HaveCount(1);
    }

    [Test]
    public async Task Handle_WithNullIdempotencyKey_ShouldCreateNewPaymentEachTime()
    {
        // Arrange
        var command = new CreatePaymentCommand(
            Guid.NewGuid(), 
            100.50m, 
            "Credit Card", 
            null);

        // Act
        var firstResult = await _handler.Handle(command, CancellationToken.None);
        var secondResult = await _handler.Handle(command, CancellationToken.None);

        // Assert
        firstResult.Should().NotBeNull();
        secondResult.Should().NotBeNull();
        secondResult.Id.Should().NotBe(firstResult.Id);

        var allPayments = await _paymentRepository.GetAllAsync();
        allPayments.Should().HaveCount(2);
    }
}

// In-memory test implementations
public class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly List<Payment> _payments = new();

    public async Task AddAsync(Payment payment, CancellationToken ct)
    {
        _payments.Add(payment);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await Task.CompletedTask;
    }

    public async Task<ITransactionScope> BeginTransactionAsync(CancellationToken ct = default)
    {
        return await Task.FromResult(new InMemoryTransactionScope());
    }

    public async Task<List<Payment>> GetAllAsync()
    {
        return await Task.FromResult(_payments.ToList());
    }
}

public class InMemoryTransactionScope : ITransactionScope
{
    public async Task CommitAsync(CancellationToken ct = default)
    {
        await Task.CompletedTask;
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await Task.CompletedTask;
    }
}

public class InMemoryMetricsTracker : IMetricsTracker
{
    public int PaymentCreatedCount { get; private set; }
    public int IdempotencyHitCount { get; private set; }

    public void IncrementPaymentCreated()
    {
        PaymentCreatedCount++;
    }

    public void IncrementIdempotencyHit()
    {
        IdempotencyHitCount++;
    }
}

public class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly Dictionary<string, PaymentDto> _store = new();

    public async Task<(bool Found, PaymentDto? Response)> TryGetAsync(string key, CancellationToken ct)
    {
        if (_store.TryGetValue(key, out var response))
        {
            return await Task.FromResult<(bool, PaymentDto?)>((true, response));
        }
        return await Task.FromResult<(bool, PaymentDto?)>((false, null));
    }

    public async Task SaveAsync(string key, PaymentDto response, CancellationToken ct)
    {
        _store[key] = response;
        await Task.CompletedTask;
    }
}
