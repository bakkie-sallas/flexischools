using Microsoft.EntityFrameworkCore.Storage;
using SchoolFees.Application.Payments;

namespace SchoolFees.Infrastructure.Repositories;

public class EfTransactionScope : ITransactionScope
{
    private readonly IDbContextTransaction _dbTransaction;
    private bool _isDisposed = false;

    public EfTransactionScope(IDbContextTransaction dbTransaction)
    {
        _dbTransaction = dbTransaction;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _dbTransaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _dbTransaction.RollbackAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (!_isDisposed)
        {
            await _dbTransaction.DisposeAsync();
            _isDisposed = true;
        }
    }
}