namespace PharmaSaaS.Application.Contracts.Invoices;

public record InvoiceItemResponse(
    int Id,
    int ProductId,
    string ProductName,
    float Quantity,
    decimal UnitPrice,
    decimal Subtotal
);
