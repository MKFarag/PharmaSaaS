namespace PharmaSaaS.Domain.Entities.Shared;

public sealed class SubscriptionRequest
{
    public string Id { get; set; } = Guid.CreateVersion7().ToString();
    public string PharmacyName { get; private set; } = string.Empty;
    public string OwnerName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public SubscriptionRequestStatus Status { get; private set; }

    public static SubscriptionRequest Create(string pharmacyName, string ownerName, string phoneNumber, string email)
        => new()
        {
            PharmacyName = pharmacyName,
            OwnerName = ownerName,
            PhoneNumber = phoneNumber,
            Email = email,
            CreatedAt = DateTime.UtcNow,
            Status = SubscriptionRequestStatus.Pending
        };

    public void Approve() => Status = SubscriptionRequestStatus.Approved;

    public void Reject() => Status = SubscriptionRequestStatus.Rejected;

    public void Contact() => Status = SubscriptionRequestStatus.Contacted;
}

