using SchoolFees.Domain;

namespace SchoolFees.Application.Payments;
public class CreatePaymentHandler
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly IMetricsTracker _metricsTracker;

    public CreatePaymentHandler(IPaymentRepository paymentRepository, IIdempotencyStore idempotencyStore, IMetricsTracker metricsTracker)
    {
        _paymentRepository = paymentRepository;
        _idempotencyStore = idempotencyStore;
        _metricsTracker = metricsTracker;
    }

    public async Task<PaymentDto> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        // Check if we've already processed this request
        if (!string.IsNullOrWhiteSpace(command.IdempotencyKey))
        {
            var existingResult = await _idempotencyStore.TryGetAsync(command.IdempotencyKey!, cancellationToken);
            if (existingResult.Item1 && existingResult.Item2 != null)
            {
                _metricsTracker.IncrementIdempotencyHit();
                return existingResult.Item2;
            }
        }

        // Start a database transaction to ensure consistency
        await using var dbTransaction = await _paymentRepository.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Create the payment domain object
            var newPayment = new Payment(command.StudentId, command.Amount, command.Method);
            
            await _paymentRepository.AddAsync(newPayment, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            // Convert to DTO for response
            var paymentDto = new PaymentDto(newPayment.Id, newPayment.StudentId, newPayment.Amount, newPayment.Method, newPayment.CreatedAt);
            
            // Store for idempotency if key provided
            if (!string.IsNullOrWhiteSpace(command.IdempotencyKey))
            {
                await _idempotencyStore.SaveAsync(command.IdempotencyKey!, paymentDto, cancellationToken);
            }

            await dbTransaction.CommitAsync(cancellationToken);
            _metricsTracker.IncrementPaymentCreated();
            
            return paymentDto;
        }
        catch
        {
            // Rollback on any error
            await dbTransaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
