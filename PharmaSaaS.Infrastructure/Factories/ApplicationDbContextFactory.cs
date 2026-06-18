using Microsoft.EntityFrameworkCore.Design;

namespace PharmaSaaS.Infrastructure.Factories;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=.;Database=DesignTimeDb;Trusted_Connection=True;")
            .Options;

        return new ApplicationDbContext(options, new DesignTimeTenantService());
    }
}

public class DesignTimeTenantService : ITenantService
{
    public string? GetConnectionString() => "Server=.;Database=DesignTimeDb;Trusted_Connection=True;";
    public Tenant? GetCurrentTenant() => null;
}
