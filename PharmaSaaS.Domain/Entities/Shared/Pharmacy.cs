namespace PharmaSaaS.Domain.Entities.Shared;

public sealed class Pharmacy
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public DateOnly? SubscriptionStart { get; private set; }
    public DateOnly? SubscriptionEnd { get; private set; }
    public bool IsActive { get; private set; }

    public static Pharmacy Create(string name)
        => new() { Name = name };

    public void Update(string name)
        => Name = name;

    public void SetSubscriptionEnd(DateOnly end)
        => SubscriptionEnd = end;

    public void ToggleStatus()
        => IsActive = !IsActive;
}
