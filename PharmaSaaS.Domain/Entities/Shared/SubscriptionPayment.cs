namespace PharmaSaaS.Domain.Entities.Shared;

public sealed class SubscriptionPayment
{
    public int Id { get; set; }
    public string PharmacyId { get; private set; } = string.Empty;
    public string ImageUrl { get; private set; } = string.Empty;
    public bool IsVerified { get; private set; }
    public decimal? Amount { get; private set; }
    public DateTime SubmittedAt { get; private set; }

    public Pharmacy? Pharmacy { get; private set; }

    public static SubscriptionPayment Create(string pharmacyId, string imageUrl)
        => new()
        {
            PharmacyId = pharmacyId,
            ImageUrl = imageUrl,
            IsVerified = false,
            SubmittedAt = DateTime.UtcNow
        };

    public void Verify(decimal? amount = null)
    {
        IsVerified = true;
        if (amount.HasValue)
            Amount = amount.Value;
    }
}
