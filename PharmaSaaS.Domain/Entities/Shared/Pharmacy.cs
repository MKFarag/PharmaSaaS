namespace PharmaSaaS.Domain.Entities.Shared;

public sealed class Pharmacy
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public DateTime? SubscriptionStart { get; private set; }
    public DateTime? SubscriptionEnd { get; private set; }
    public bool IsActive { get; private set; }

    public static Pharmacy Create(string name)
        => new() { Name = name };

    public void SetSubscription(DateTime start, DateTime end)
    {
        SubscriptionStart = start;
        SubscriptionEnd = end;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
