namespace SchoolFees.Application.Payments;

public interface IMetricsTracker
{
    void IncrementPaymentCreated();
    void IncrementIdempotencyHit();
}