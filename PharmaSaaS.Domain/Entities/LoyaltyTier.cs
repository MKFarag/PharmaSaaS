namespace PharmaSaaS.Domain.Entities;

public sealed class LoyaltyTier
{
    public int Id { get; set; }
    public string Name { get; private set; } = string.Empty;
    public decimal SpendThreshold { get; private set; }

    public ICollection<DiscountRule> DiscountRules { get; set; } = new List<DiscountRule>();

    public static LoyaltyTier Create(string name, decimal spendThreshold)
        => new()
        {
            Name = name,
            SpendThreshold = spendThreshold
        };

    public void Update(string name, decimal spendThreshold)
    {
        Name = name;
        SpendThreshold = spendThreshold;
    }
}
