using SchoolFees.Application.Payments;
using SchoolFees.Domain;

namespace SchoolFees.UnitTests.Application;

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