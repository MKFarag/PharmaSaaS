namespace PharmaSaaS.Infrastructure.Persistence.Identities;

public sealed class ApplicationRole : IdentityRole
{
    public ApplicationRole()
    {
        Id = Guid.CreateVersion7().ToString();
    }

    public bool IsDisabled { get; set; }
    public bool IsDefault { get; set; }
}
