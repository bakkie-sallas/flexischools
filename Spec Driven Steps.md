### PART 1 Architecture Added
context Diagram
Sequence Diagram 
context.md


#### Create solution + projects

1.  Create a new solution `SchoolFees.sln`.
    
2.  Add projects:
    
    -   `SchoolFees.Domain`
        
    -   `SchoolFees.Application`
        
    -   `SchoolFees.Infrastructure`
        
    -   `SchoolFees.Api`
        
    -   `SchoolFees.UnitTests`
        
    -   `SchoolFees.IntegrationTests`
        
3.  Reference order (onion style):
    
    -   **Domain** -> no dependencies.
        
    -   **Application**-> references Domain.
        
    -   **Infrastructure** -> references Application + Domain.
        
    -   **Api** -> references Application + Infrastructure.
        
    -   **Tests** ->reference all as needed.
#### Domain Layer

1.  In `SchoolFees.Domain`, create the entities/value objects:
    
    -   `Payment`
        
    `Payment 
	{ 
	    public Guid Id 
	    public Guid StudentId
	    public  decimal Amount 
	    public  string Method
	    public DateTime CreatedAt 
	    private  Payment() { } 
	    public  Payment(Guid studentId, decimal amount, string
					    method)
		}` 
        
2.  Define supporting records if needed:
    
    -   `StudentId` as value object.
        
    -   Enum `PaymentMethod { Card, DirectDebit }`
	
	
	
#### Application Layer (CQRS)

1.  In `SchoolFees.Application/Payments`, add:
    
    -   Command: `CreatePaymentCommand(Guid StudentId, decimal Amount, string Method, string? IdempotencyKey)`.
        
    -   DTO: `PaymentDto(Guid Id, Guid StudentId, decimal Amount, string Method, DateTimeOffset CreatedAt)`.
        
    -   Interfaces:
        
`IPaymentRepository 
{ 
    AddAsync(Payment payment, CancellationToken ct); 
    SaveChangesAsync(CancellationToken ct);
} 
IIdempotencyStore 
{
	TryGetAsync(string key, CancellationToken ct); 
	SaveAsync(string key, PaymentDto response, 		CancellationToken ct);
}` 
   -   Handler:
        
`CreatePaymentHandler {IPaymentRepository _repo; IIdempotencyStore _idem; 
CreatePaymentHandler(IPaymentRepository repo, IIdempotencyStore idem) => somedelegate; 
Handle(CreatePaymentCommand cmd, CancellationToken ct)
{ <<access and save to dto>>}`



#### Infrastructure Layer

1.  Add EF Core setup:
    
    -   `FeesDbContext` with 
    - `DbSet<Payment>`
    - `DbSet<IdempotencyRecord>`.
        
    - `IdempotencyRecord` entity with 
    - `Key`, `ResponseJson`, `CreatedAtUtc`.
        
2.  Implement:
    
    -   `EfPaymentRepository` -> implements `IPaymentRepository`.
        
    -   `EfIdempotencyStore` -> implements `IIdempotencyStore`.
        
3.  Configure EF to use **SQLite In-Memory**:
    
    `options.UseSqlite("DataSource=:memory:");`
	
	
#### Testing

1.  **Unit tests (NUnit + FluentAssertions)**
    
    -   Validate domain rule: payment amount > 0.
        
    -   Validate handler returns same result for same Idempotency-Key.
	
	
	
#### API Layer (Minimal API)

1.  In `SchoolFees.Api/Program.cs`:
    
-   Configure DI:
        
`DbContext FeesDbContext Sqlite("DataSource=:memory:")
Scoped IPaymentRepository= EfPaymentRepository
Scoped IIdempotencyStore= EfIdempotencyStore
Scoped CreatePaymentHandler` 
        
-   Endpoint:
        
`map "/fees/{studentId:guid}/payments", to CreatePaymentCommand()`



**Integration test**

-   Use some factory to POST to `/fees/{studentId}/payments`.
    
-   Assert `201 Created` and same ID on repeated calls with same key.