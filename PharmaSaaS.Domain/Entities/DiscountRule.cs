namespace PharmaSaaS.Domain.Entities;

public sealed class DiscountRule
{
    public int Id { get; set; }
    public int LoyaltyTierId { get; private set; }
    public decimal MinInvoiceAmount { get; private set; }
    public decimal DiscountPercent { get; private set; }

    public LoyaltyTier LoyaltyTier { get; set; } = default!;

    public static DiscountRule Create(int loyaltyTierId, decimal minInvoiceAmount, decimal discountPercent)
        => new()
        {
            LoyaltyTierId = loyaltyTierId,
            MinInvoiceAmount = minInvoiceAmount,
            DiscountPercent = discountPercent
        };

    public void Update(decimal minInvoiceAmount, decimal discountPercent)
    {
        MinInvoiceAmount = minInvoiceAmount;
        DiscountPercent = discountPercent;
    }
}
