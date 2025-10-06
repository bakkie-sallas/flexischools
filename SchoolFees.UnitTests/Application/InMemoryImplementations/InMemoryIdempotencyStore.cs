using SchoolFees.Application.Payments;

namespace SchoolFees.UnitTests.Application;


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