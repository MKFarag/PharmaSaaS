namespace PharmaSaaS.Domain.Entities;

public sealed class SupplierPayment
{
    public int Id { get; set; }
    public decimal Amount { get; private set; }
    public DateTime PaidAt { get; private set; }
    public string? Notes { get; private set; }

    public int SupplierId { get; private set; }
    public Supplier Supplier { get; private set; } = default!;

    public ICollection<PurchaseOrder> Orders { get; private set; } = default!;

    public static SupplierPayment Create(int supplierId, decimal amount, string? notes)
        => new()
        {
            SupplierId = supplierId,
            Amount = amount,
            Notes = notes,
            PaidAt = DateTime.UtcNow
        };

    public void SetSupplier(Supplier supplier) => Supplier = supplier;
    public void SetOrders(ICollection<PurchaseOrder> orders) => Orders = orders;
}

