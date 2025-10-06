

# SchoolFees – Pay Now API Slice (DDD / Onion Architecture)

This project implements a small **Domain-Driven Design (DDD)** and **Onion Architecture** slice for handling school fee payments.  
It demonstrates clean layering, idempotency, CQRS-style application logic, EF Core persistence, and minimal API design.

---

## Use Case

**Endpoint:** `POST /fees/{studentId}/payments`  
**Request body:**  
```json
{
  "amount": 120.50,
  "method": "Card"
}
```
Behavior:

Creates a new payment record and returns it.

Prevents duplicate payments using the Idempotency-Key header.

Persists data in an EF Core SQLite (in-memory) database.

Layers
Layer	Description
Domain	Contains core entities and value objects. Enforces business rules and invariants.
Application	Implements commands, handlers, and interfaces (CQRS-style). Contains no infrastructure dependencies.
Infrastructure	Provides EF Core persistence, repositories, and idempotency store.
API	Minimal ASP.NET Core project exposing endpoints and dependency injection setup.
Tests	Unit and integration tests using NUnit + FluentAssertions.
t outer layers.


**Clone and build**
 - bash   
 - git clone https://github.com/yourname/SchoolFees.git  
 - cd   SchoolFees   
 - dotnet build

**Run the API**
 - cd SchoolFees.Api  
 - dotnet run

**Test the endpoint**

in powershell 7:


$headers = @{
  "Content-Type" = "application/json"
  "Idempotency-Key" = "abc-123"
}

$body = '{"amount":120.50,"method":"Card"}'

Invoke-RestMethod -Uri "http://localhost:5054/fees/6c47f301-d911-4c3b-81ff-8b3fc08e6163/payments" `
  -Method POST `
  -Headers $headers `
  -Body $body `
  
  
  dotnet dev-certs https --trust
  Invoke-RestMethod -Uri "https://localhost:7042/fees/6c47f301-d911-4c3b-81ff-8b3fc08e6163/payments" `
  -Method POST `
  -Headers $headers `
  -Body $body `
  -SkipCertificateCheck
Response:

```json
{
  "id": "6c47f301-d911-4c3b-81ff-8b3fc08e6163",
  "studentId": "GUID",
  "amount": 120.50,
  "method": "Card",
  "createdAt": "2025-10-05T00:00:00Z"
}
```
