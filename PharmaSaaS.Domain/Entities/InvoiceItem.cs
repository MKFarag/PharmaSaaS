namespace PharmaSaaS.Domain.Entities;

public sealed class InvoiceItem
{
    public int Id { get; set; }
    public int InvoiceId { get; private set; }
    public int ProductId { get; private set; }
    
    public float Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    
    public decimal Subtotal => (decimal)Quantity * UnitPrice;

    public Invoice Invoice { get; set; } = default!;
    public Product Product { get; set; } = default!;

    public static InvoiceItem Create(int invoiceId, int productId, float quantity, decimal unitPrice)
    {
        return new InvoiceItem
        {
            InvoiceId = invoiceId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}
