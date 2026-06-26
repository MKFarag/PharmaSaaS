namespace PharmaSaaS.Domain.Entities;

public sealed class Inventory
{
    public int Id { get; set; }
    public int Quantity { get; private set; }
    public decimal PurchasePrice { get; private set; }
    public int? TargetStock { get; private set; }
    public bool Rare { get; private set; }
    public int ProductId { get; private set; }

    public Product Product { get; set; } = default!;

    public static Inventory Create(int productId, int quantity, decimal purchasePrice, int? targetStock = null)
        => new()
        {
            ProductId = productId,
            Quantity = quantity,
            PurchasePrice = purchasePrice,
            TargetStock = targetStock
        };

    public void SetQuantity(int quantity) => Quantity = quantity;
    public void SetPurchasePrice(decimal purchasePrice) => PurchasePrice = purchasePrice;
    public void SetTargetStock(int? targetStock) => TargetStock = targetStock;
    public void MarkRare(bool rare) => Rare = rare;
}
