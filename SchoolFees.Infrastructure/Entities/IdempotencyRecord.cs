using System.ComponentModel.DataAnnotations;

namespace SchoolFees.Infrastructure.Entities;

public class IdempotencyRecord
{
    [Key]
    public string Key { get; set; } = string.Empty;
    
    public string ResponseJson { get; set; } = string.Empty;
    
    public DateTime CreatedAtUtc { get; set; }
}
