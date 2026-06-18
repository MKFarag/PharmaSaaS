namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

internal class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    {
        // CHECK: Uncomment this.
        // Default data

        //builder
        //    .HasData(new IdentityUserRole<string>
        //    {
        //        RoleId = DefaultRoles.Admin.Id,
        //        UserId = DefaultUsers.Admin.Id
        //    });
    }
}
