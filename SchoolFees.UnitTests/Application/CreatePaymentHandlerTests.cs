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




