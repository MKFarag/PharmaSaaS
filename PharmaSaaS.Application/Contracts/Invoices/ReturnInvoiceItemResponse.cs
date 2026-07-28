namespace PharmaSaaS.Application.Contracts.Invoices;

public record ReturnInvoiceItemResponse(
    int ProductId,
    string ProductName,
    float Quantity,
    decimal RefundAmount
);
