namespace PharmaSaaS.Domain.Entities;

public sealed class LoyaltyTier
{
    public int Id { get; set; }
    public string Name { get; private set; } = string.Empty;
    public decimal SpendThreshold { get; private set; }

    public ICollection<DiscountRule> DiscountRules { get; private set; } = [];

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

    /// <summary>
    /// Validates that no two rules share the same MinInvoiceAmount, then adds them.
    /// Returns false if duplicates are found.
    /// </summary>
    public bool TryAddRules(IEnumerable<(decimal MinInvoiceAmount, decimal DiscountPercent)> rules)
    {
        var ruleList = rules.ToList();

        if (ruleList.GroupBy(r => r.MinInvoiceAmount).Any(g => g.Count() > 1))
            return false;

        foreach (var (minAmount, discountPercent) in ruleList)
            DiscountRules.Add(DiscountRule.Create(0, minAmount, discountPercent));

        return true;
    }
}

