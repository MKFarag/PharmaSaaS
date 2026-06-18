namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

internal class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        // CHECK: Uncomment this.
        // Default data

        //builder.HasData(new ApplicationRole
        //{
        //    Id = DefaultRoles.Admin.Id,
        //    Name = DefaultRoles.Admin.Name,
        //    NormalizedName = DefaultRoles.Admin.Name.ToUpper(),
        //    ConcurrencyStamp = DefaultRoles.Admin.ConcurrencyStamp,
        //    IsDefault = false,
        //},
        //new ApplicationRole
        //{
        //    Id = DefaultRoles.Default.Id,
        //    Name = DefaultRoles.Default.Name,
        //    NormalizedName = DefaultRoles.Default.Name.ToUpper(),
        //    ConcurrencyStamp = DefaultRoles.Default.ConcurrencyStamp,
        //    IsDefault = true,
        //});
    }
}
