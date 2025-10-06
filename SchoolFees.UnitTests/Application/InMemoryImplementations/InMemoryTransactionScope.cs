using SchoolFees.Application.Payments;

namespace SchoolFees.UnitTests.Application;


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