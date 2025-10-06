using SchoolFees.Application.Payments;

namespace SchoolFees.Api.Services;

/*
The MetricsService provides a centralized way to collect and log application metrics related to payment processing operations.
*/
public class MetricsService : IMetricsService, IMetricsTracker
{
    private readonly ILogger<MetricsService> _logger;
    private long _paymentCreatedCount = 0;
    private long _idempotencyHitCount = 0;

    public MetricsService(ILogger<MetricsService> logger)
    {
        _logger = logger;
    }
    //The MetricsService provides a centralized way to collect and log application metrics related to payment processing operations.
    //Thread Safety: Uses `Interlocked.Increment()` to ensure thread-safe counter operations in concurrent scenarios
    public void IncrementPaymentCreated()
    {
        Interlocked.Increment(ref _paymentCreatedCount);
        _logger.LogInformation("Metric: payment.created.count incremented to {Count}", _paymentCreatedCount);
    }

    public void IncrementIdempotencyHit()
    {
        Interlocked.Increment(ref _idempotencyHitCount);
        _logger.LogInformation("Metric: idempotency.hit.count incremented to {Count}", _idempotencyHitCount);
    }
}
