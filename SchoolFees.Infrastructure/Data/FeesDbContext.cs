using Microsoft.EntityFrameworkCore;
using SchoolFees.Domain;
using SchoolFees.Infrastructure.Entities;

namespace SchoolFees.Infrastructure.Data;

public class FeesDbContext : DbContext
{
    public FeesDbContext(DbContextOptions<FeesDbContext> options) : base(options)
    {
    }

    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<IdempotencyRecord> IdempotencyRecords { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Payment entity
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StudentId).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Method).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        // Configure IdempotencyRecord entity
        modelBuilder.Entity<IdempotencyRecord>(entity =>
        {
            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key).HasMaxLength(255);
            entity.Property(e => e.ResponseJson).IsRequired();
            entity.Property(e => e.CreatedAtUtc).IsRequired();
        });
    }
}
