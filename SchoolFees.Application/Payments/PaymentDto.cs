namespace SchoolFees.Application.Payments;

public record PaymentDto(
    Guid Id, 
    Guid StudentId, 
    decimal Amount, 
    string Method, 
    DateTimeOffset CreatedAt);
