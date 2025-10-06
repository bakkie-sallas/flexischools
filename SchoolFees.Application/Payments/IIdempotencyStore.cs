namespace SchoolFees.Application.Payments;

public interface IIdempotencyStore
{
    Task<(bool Found, PaymentDto? Response)> TryGetAsync(string key, CancellationToken ct);
    Task SaveAsync(string key, PaymentDto response, CancellationToken ct);
}
