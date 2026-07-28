namespace PharmaSaaS.Domain.Entities;

public sealed class ReturnInvoice
{
    public int Id { get; set; }
    public int OriginalInvoiceId { get; private set; }
    public int ShiftId { get; private set; }
    
    public decimal TotalRefund { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }

    public Invoice OriginalInvoice { get; set; } = default!;
    public Shift Shift { get; set; } = default!;

    public ICollection<ReturnInvoiceItem> Items { get; set; } = [];

    public static ReturnInvoice Create(int originalInvoiceId, int shiftId, decimal totalRefund)
    {
        return new ReturnInvoice
        {
            OriginalInvoiceId = originalInvoiceId,
            ShiftId = shiftId,
            TotalRefund = totalRefund,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
