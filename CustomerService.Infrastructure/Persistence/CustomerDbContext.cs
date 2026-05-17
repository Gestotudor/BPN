using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Persistence;

public sealed class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<CustomerStatusHistory> CustomerStatusHistories => Set<CustomerStatusHistory>();

    public DbSet<KycVerificationLog> KycVerificationLogs => Set<KycVerificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerDbContext).Assembly);
    }
}
