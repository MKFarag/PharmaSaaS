namespace PharmaSaaS.Domain.Entities;

public sealed class Role
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsDisabled { get; private set; }
    public bool IsDefault { get; private set; }

    public static Role Create(string name)
        => new() { Name = name };

    public void Update(string name)
        => Name = name;

    public void ToggleStatus()
        => IsDisabled = !IsDisabled;
}
