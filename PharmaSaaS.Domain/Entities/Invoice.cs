namespace PharmaSaaS.Domain.Entities;

public sealed class Invoice
{
    public int Id { get; set; }
    public int ShiftId { get; private set; }
    public int? CustomerId { get; private set; }

    public decimal Total { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal FinalAmount { get; private set; }

    public int ItemCount { get; private set; }
    public bool HasReturn { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public Shift Shift { get; private set; } = default!;
    public Customer? Customer { get; private set; }

    public ICollection<InvoiceItem> Items { get; private set; } = [];
    public ICollection<Return> Returns { get; private set; } = [];

    public static readonly string[] DetailIncludes = [
        nameof(Items),
        $"{nameof(Items)}.{nameof(InvoiceItem.Product)}",
        nameof(Customer),
        nameof(Shift),
        nameof(Returns),
        $"{nameof(Returns)}.{nameof(Return.Product)}"
    ];

    public static Invoice Create(int shiftId, int? customerId, decimal total, decimal discountAmount, int itemCount)
    {
        var now = DateTime.UtcNow;
        return new Invoice
        {
            ShiftId = shiftId,
            CustomerId = customerId,
            Total = total,
            DiscountAmount = discountAmount,
            FinalAmount = total - discountAmount,
            ItemCount = itemCount,
            HasReturn = false,
            CreatedAt = now
        };
    }

    public void MarkHasReturn() => HasReturn = true;
}

