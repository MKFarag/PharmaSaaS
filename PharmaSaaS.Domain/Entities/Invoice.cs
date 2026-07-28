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
    
    public DateOnly Date { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Shift Shift { get; set; } = default!;
    public Customer? Customer { get; set; }
    
    public ICollection<InvoiceItem> Items { get; set; } = [];
    public ICollection<ReturnInvoice> ReturnInvoices { get; set; } = [];

    public static readonly string[] DetailIncludes = [
        nameof(Items),
        $"{nameof(Items)}.{nameof(InvoiceItem.Product)}",
        nameof(Customer),
        nameof(ReturnInvoices),
        $"{nameof(ReturnInvoices)}.{nameof(ReturnInvoice.Items)}",
        $"{nameof(ReturnInvoices)}.{nameof(ReturnInvoice.Items)}.{nameof(ReturnInvoiceItem.Product)}"
    ];

    public static Invoice Create(int shiftId, int? customerId, decimal total, decimal discountAmount, int itemCount)
    {
        var now = DateTimeOffset.UtcNow;
        return new Invoice
        {
            ShiftId = shiftId,
            CustomerId = customerId,
            Total = total,
            DiscountAmount = discountAmount,
            FinalAmount = total - discountAmount,
            ItemCount = itemCount,
            HasReturn = false,
            Date = DateOnly.FromDateTime(now.DateTime),
            CreatedAt = now
        };
    }

    public void MarkHasReturn() => HasReturn = true;
}
