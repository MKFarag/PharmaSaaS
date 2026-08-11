namespace PharmaSaaS.Domain.Entities;

public sealed class Customer
{
    public int Id { get; set; }
    public string FullName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public decimal TotalSpend { get; private set; }

    public int? CurrentTierId { get; private set; }
    public LoyaltyTier? CurrentTier { get; private set; }

    public static Customer Create(string fullName, string phoneNumber)
        => new()
        {
            FullName = fullName,
            PhoneNumber = phoneNumber,
            TotalSpend = 0,
            CurrentTierId = null
        };

    public void Update(string fullName, string phoneNumber)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
    }

    public void UpdateSpendAndTier(decimal totalSpend, int? currentTierId)
    {
        TotalSpend = totalSpend;
        CurrentTierId = currentTierId;
    }
}

