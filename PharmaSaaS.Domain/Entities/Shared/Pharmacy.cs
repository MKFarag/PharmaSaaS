namespace PharmaSaaS.Domain.Entities.Shared;

public sealed class Pharmacy
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string OwnerName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateOnly? SubscriptionStart { get; private set; }
    public DateOnly? SubscriptionEnd { get; private set; }
    public bool IsActive { get; private set; }
    public string? CustomAiApiKey { get; private set; }

    public static Pharmacy Create(string name, string ownerName, string phoneNumber, string email, DateOnly subscriptionEnd)
    {
        var subscriptionStart = DateOnly.FromDateTime(DateTime.UtcNow);

        return new Pharmacy
        {
            Id = Guid.CreateVersion7().ToString(),
            Name = name,
            OwnerName = ownerName,
            PhoneNumber = phoneNumber,
            Email = email,
            TenantId = $"{Guid.NewGuid():N}"[..16],
            SubscriptionStart = subscriptionStart,
            SubscriptionEnd = subscriptionEnd,
            IsActive = true
        };
    }

    public void Update(string name)
        => Name = name;

    public void SetSubscriptionEnd(DateOnly end)
        => SubscriptionEnd = end;

    public void ToggleStatus()
        => IsActive = !IsActive;

    public void Deactivate()
        => IsActive = false;

    public void SetCustomAiApiKey(string? apiKey)
        => CustomAiApiKey = apiKey;
}
