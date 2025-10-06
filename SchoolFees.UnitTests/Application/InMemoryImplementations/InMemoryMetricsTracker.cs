using SchoolFees.Application.Payments;

namespace SchoolFees.UnitTests.Application;


public class InMemoryMetricsTracker : IMetricsTracker
{
    public int PaymentCreatedCount { get; private set; }
    public int IdempotencyHitCount { get; private set; }

    public void IncrementPaymentCreated()
    {
        PaymentCreatedCount++;
    }

    public void IncrementIdempotencyHit()
    {
        IdempotencyHitCount++;
    }
}