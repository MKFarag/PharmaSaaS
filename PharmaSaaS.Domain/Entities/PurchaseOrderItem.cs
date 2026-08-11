using PharmaSaaS.Domain.Enums;

namespace PharmaSaaS.Domain.Entities;

public sealed class PurchaseOrderItem
{
    public int Id { get; set; }
    public float Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public float ReceivedQuantity { get; private set; }
    public float ReturnedQuantity { get; private set; }
    public string? ReturnReason { get; private set; }
    public PurchaseOrderItemStatus Status { get; private set; }

    public int ProductId { get; private set; }
    public Product Product { get; private set; } = default!;

    public int PurchaseOrderId { get; private set; }
    public PurchaseOrder PurchaseOrder { get; private set; } = default!;

    public static PurchaseOrderItem Create(int productId, float quantity, decimal unitCost = 0)
        => new()
        {
            ProductId = productId,
            Quantity = quantity,
            UnitCost = unitCost,
            ReceivedQuantity = 0,
            ReturnedQuantity = 0,
            Status = PurchaseOrderItemStatus.Pending
        };

    public void Approve(decimal finalUnitCost, float receivedQuantity, bool isMissing = false)
    {
        UnitCost = finalUnitCost;
        ReceivedQuantity = receivedQuantity;
        
        if (isMissing || receivedQuantity == 0)
            Status = PurchaseOrderItemStatus.Missing;
        else
            Status = PurchaseOrderItemStatus.Received;
    }

    public void Return(float quantityToReturn, string reason)
    {
        ReturnedQuantity += quantityToReturn;
        ReceivedQuantity -= quantityToReturn; // Deduct from the actually kept amount
        ReturnReason = reason;

        if (ReceivedQuantity <= 0)
            Status = PurchaseOrderItemStatus.Returned;
    }
    public void AddQuantity(float extraQuantity)
    {
        Quantity += extraQuantity;
    }
}

