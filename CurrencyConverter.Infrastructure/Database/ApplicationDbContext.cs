using CurrencyConverter.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurrencyConverter.Infrastructure.Database;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<ExchangeRate> ExchangeRates { get; set; } = null!;
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Application);
        
        modelBuilder.Entity<ExchangeRate>()
            .HasIndex(e => new { e.BaseCurrency, e.TargetCurrency, e.CreatedAt })
            .IsUnique();
    }
}
