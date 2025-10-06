namespace SchoolFees.Application.Payments;

public record CreatePaymentCommand(
    Guid StudentId, 
    decimal Amount, 
    string Method, 
    string? IdempotencyKey);
