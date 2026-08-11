using PharmaSaaS.Domain.Enums;

namespace PharmaSaaS.Domain.Entities;

public sealed class PurchaseOrder
{
    public int Id { get; set; }
    public DateOnly OrderDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }

    public int SupplierId { get; private set; }
    public Supplier Supplier { get; private set; } = default!;

    public ICollection<PurchaseOrderItem> Items { get; private set; } = default!;

    public decimal RemainingAmount => TotalAmount - PaidAmount - ReturnedAmount;

    public decimal CalculatedAmount => Items?.Sum(i => (decimal)i.ReceivedQuantity * i.UnitCost) ?? 0;
    public decimal ReturnedAmount => Items?.Sum(i => (decimal)i.ReturnedQuantity * i.UnitCost) ?? 0;

    public static PurchaseOrder Create(int supplierId, decimal totalAmount, decimal paidAmount)
        => new()
        {
            SupplierId = supplierId,
            TotalAmount = totalAmount,
            PaidAmount = paidAmount,
            Status = PurchaseOrderStatus.Pending,
            OrderDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

    public void Receive() => Status = PurchaseOrderStatus.Received;
    public void AdjustAmount(decimal newTotalAmount) => TotalAmount = newTotalAmount;
    public void RecordPayment(decimal amount) => PaidAmount += amount;
    public void SetSupplier(Supplier supplier) => Supplier = supplier;
    public void SetItems(ICollection<PurchaseOrderItem> items) => Items = items;
}

