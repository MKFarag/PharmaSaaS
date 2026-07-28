namespace PharmaSaaS.Domain.Entities;

public sealed class ReturnInvoiceItem
{
    public int Id { get; set; }
    public int ReturnInvoiceId { get; private set; }
    public int ProductId { get; private set; }
    
    public float Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal RefundAmount { get; private set; }

    public ReturnInvoice ReturnInvoice { get; set; } = default!;
    public Product Product { get; set; } = default!;

    public static ReturnInvoiceItem Create(int returnInvoiceId, int productId, float quantity, decimal unitPrice)
    {
        return new ReturnInvoiceItem
        {
            ReturnInvoiceId = returnInvoiceId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            RefundAmount = (decimal)quantity * unitPrice
        };
    }
}
