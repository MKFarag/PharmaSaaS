namespace PharmaSaaS.Infrastructure.Persistence;

public class SharedDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Pharmacy> Pharmacies { get; set; }
    public DbSet<SubscriptionRequest> SubscriptionRequests { get; set; }
    public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(),
            type => type.Namespace?.Contains("Configurations.Shared") == true);
    }
}
