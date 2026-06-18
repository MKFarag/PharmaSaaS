using Microsoft.EntityFrameworkCore.Design;

namespace PharmaSaaS.Infrastructure.Factories;

public class SharedDbContextFactory : IDesignTimeDbContextFactory<SharedDbContext>
{
    public SharedDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SharedDbContext>()
            .UseSqlServer("Server=.;Database=PharmaSaaS;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        return new SharedDbContext(options);
    }
}
