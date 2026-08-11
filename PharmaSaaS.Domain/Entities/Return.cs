namespace PharmaSaaS.Domain.Entities;

public sealed class Return
{
    public int Id { get; private set; }
    public int Quantity { get; private set; }
    public decimal RefundAmount { get; private set; }
    public DateTime ReturnDate { get; private set; }

    public int InvoiceId { get; private set; }
    public int ProductId { get; private set; }
    
    public Invoice Invoice { get; private set; } = default!;
    public Product Product { get; private set; } = default!;

    public static Return Create(int invoiceId, int productId, int quantity, decimal refundAmount)
        => new()
        {
            InvoiceId = invoiceId,
            ProductId = productId,
            Quantity = quantity,
            RefundAmount = refundAmount,
            ReturnDate = DateTime.UtcNow
        };
}

