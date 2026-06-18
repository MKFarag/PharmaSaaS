namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

internal class RoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<string>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<string>> builder)
    {
        // Default data

        var allPermissions = Permissions.GetAll();
        var adminClaims = new List<IdentityRoleClaim<string>>();
        var id = 1;

        // Admin claims
        foreach (var permission in allPermissions)
            adminClaims.Add(new IdentityRoleClaim<string>
            {
                Id = id++,
                RoleId = DefaultRoles.Admin.Id,
                ClaimType = Permissions.Type,
                ClaimValue = permission
            });

        // TODO: Activate when seeding is ready
        //builder.HasData(adminClaims);
    }
}
