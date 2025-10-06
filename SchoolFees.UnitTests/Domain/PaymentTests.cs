using FluentAssertions;
using SchoolFees.Domain;

namespace SchoolFees.UnitTests.Domain;

[TestFixture]
public class PaymentTests
{
    [Test]
    public void Constructor_WithValidAmount_ShouldCreatePayment()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var amount = 100.50m;
        var method = "Credit Card";

        // Act
        var payment = new Payment(studentId, amount, method);

        // Assert
        payment.Id.Should().NotBe(Guid.Empty);
        payment.StudentId.Should().Be(studentId);
        payment.Amount.Should().Be(amount);
        payment.Method.Should().Be(method);
        payment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void Constructor_WithZeroAmount_ShouldThrowArgumentException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var amount = 0m;
        var method = "Credit Card";

        // Act & Assert
        var act = () => new Payment(studentId, amount, method);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Payment amount must be greater than zero*")
           .And.ParamName.Should().Be("amount");
    }

    [Test]
    public void Constructor_WithNegativeAmount_ShouldThrowArgumentException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var amount = -50.00m;
        var method = "Credit Card";

        // Act & Assert
        var act = () => new Payment(studentId, amount, method);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Payment amount must be greater than zero*")
           .And.ParamName.Should().Be("amount");
    }

    [Test]
    public void Constructor_WithEmptyMethod_ShouldThrowArgumentException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var amount = 100.00m;
        var method = "";

        // Act & Assert
        var act = () => new Payment(studentId, amount, method);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Payment method cannot be null or empty*")
           .And.ParamName.Should().Be("method");
    }

    [Test]
    public void Constructor_WithNullMethod_ShouldThrowArgumentException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var amount = 100.00m;
        string method = null!;

        // Act & Assert
        var act = () => new Payment(studentId, amount, method);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Payment method cannot be null or empty*")
           .And.ParamName.Should().Be("method");
    }

    [Test]
    public void Constructor_WithEmptyStudentId_ShouldThrowArgumentException()
    {
        // Arrange
        var studentId = Guid.Empty;
        var amount = 100.00m;
        var method = "Credit Card";

        // Act & Assert
        var act = () => new Payment(studentId, amount, method);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Student ID cannot be empty*")
           .And.ParamName.Should().Be("studentId");
    }
}
