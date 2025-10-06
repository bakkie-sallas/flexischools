using SchoolFees.Domain;

namespace SchoolFees.Application.Payments;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task<ITransactionScope> BeginTransactionAsync(CancellationToken ct = default);
}

public interface ITransactionScope : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
