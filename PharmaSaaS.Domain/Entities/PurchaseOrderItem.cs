namespace PharmaSaaS.Domain.Entities;

public sealed class PurchaseOrderItem
{
    public int Id { get; set; }
    public float Quantity { get; private set; }
    public decimal UnitCost { get; private set; }

    public int ProductId { get; private set; }
    public Product Product { get; set; } = default!;

    public int PurchaseOrderId { get; private set; }
    public PurchaseOrder PurchaseOrder { get; set; } = default!;

    public static PurchaseOrderItem Create(int productId, float quantity, decimal unitCost)
        => new()
        {
            ProductId = productId,
            Quantity = quantity,
            UnitCost = unitCost
        };
}
