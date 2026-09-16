namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

internal class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        // Default data

        builder.HasData(new ApplicationRole
        {
            Id = DefaultRoles.Admin.Id,
            Name = DefaultRoles.Admin.Name,
            NormalizedName = DefaultRoles.Admin.Name.ToUpper(),
            ConcurrencyStamp = DefaultRoles.Admin.ConcurrencyStamp,
            IsDefault = false,
        },
        new ApplicationRole
        {
            Id = DefaultRoles.User.Id,
            Name = DefaultRoles.User.Name,
            NormalizedName = DefaultRoles.User.Name.ToUpper(),
            ConcurrencyStamp = DefaultRoles.User.ConcurrencyStamp,
            IsDefault = true,
        });
    }
}
