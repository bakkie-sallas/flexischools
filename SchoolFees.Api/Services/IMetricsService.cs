namespace SchoolFees.Api.Services;

public interface IMetricsService
{
    void IncrementPaymentCreated();
    void IncrementIdempotencyHit();
}