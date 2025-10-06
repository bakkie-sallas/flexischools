namespace SchoolFees.Domain;

public class Payment
{
    public Guid Id { get; private set; }
    public Guid StudentId { get; private set; }
    public decimal Amount { get; private set; }
    public string Method { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    // EF Core needs a parameterless constructor
    private Payment() 
    { 
        Method = string.Empty;
    }

    public Payment(Guid studentId, decimal amount, string method)
    {
        // Generate a unique ID for this payment
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;

        // Validate student ID
        if (studentId == Guid.Empty) 
        {
            throw new ArgumentException("Student ID cannot be empty", nameof(studentId));
        }
        
        // Validate payment amount
        if (amount <= 0) 
        {
            throw new ArgumentException("Payment amount must be greater than zero", nameof(amount));
        }
        
        // Validate payment method
        if (string.IsNullOrWhiteSpace(method)) 
        {
            throw new ArgumentException("Payment method cannot be null or empty", nameof(method));
        }
        
        StudentId = studentId;
        Amount = amount;
        Method = method;
    }
}
