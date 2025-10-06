using Microsoft.EntityFrameworkCore;
using SchoolFees.Application.Payments;
using SchoolFees.Infrastructure.Data;
using SchoolFees.Infrastructure.Entities;
using System.Text.Json;

namespace SchoolFees.Infrastructure.Repositories;

public class EfIdempotencyStore : IIdempotencyStore
{
    private readonly FeesDbContext _context;

    public EfIdempotencyStore(FeesDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Found, PaymentDto? Response)> TryGetAsync(string key, CancellationToken ct)
    {
        var record = await _context.IdempotencyRecords
            .FirstOrDefaultAsync(r => r.Key == key, ct);

        if (record == null)
            return (false, null);

        var response = JsonSerializer.Deserialize<PaymentDto>(record.ResponseJson);
        return (true, response);
    }

    public async Task SaveAsync(string key, PaymentDto response, CancellationToken ct)
    {
        var responseJson = JsonSerializer.Serialize(response);
        
        var record = new IdempotencyRecord
        {
            Key = key,
            ResponseJson = responseJson,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _context.IdempotencyRecords.AddAsync(record, ct);
        await _context.SaveChangesAsync(ct);
    }
}
