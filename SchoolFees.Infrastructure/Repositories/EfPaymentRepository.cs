using SchoolFees.Application.Payments;
using SchoolFees.Domain;
using SchoolFees.Infrastructure.Data;

namespace SchoolFees.Infrastructure.Repositories;

public class EfPaymentRepository : IPaymentRepository
{
    private readonly FeesDbContext _dbContext;

    public EfPaymentRepository(FeesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken)
    {
        await _dbContext.Payments.AddAsync(payment, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ITransactionScope> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // Start a new database transaction
        var dbTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new EfTransactionScope(dbTransaction);
    }
}


