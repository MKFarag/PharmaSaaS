namespace PharmaSaaS.Domain.Entities;

public sealed class PurchaseOrder
{
    public int Id { get; set; }
    public DateOnly OrderDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Status { get; private set; } = string.Empty;

    public int SupplierId { get; private set; }
    public Supplier Supplier { get; set; } = default!;

    public ICollection<PurchaseOrderItem> Items { get; set; } = default!;

    public static PurchaseOrder Create(int supplierId, decimal totalAmount)
        => new()
        {
            SupplierId = supplierId,
            TotalAmount = totalAmount,
            Status = Constants.PurchaseOrderStatus.Pending,
            OrderDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

    public void Receive()
    {
        Status = Constants.PurchaseOrderStatus.Received;
    }
}
