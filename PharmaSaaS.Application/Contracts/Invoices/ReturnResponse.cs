namespace PharmaSaaS.Application.Contracts.Invoices;

public record ReturnResponse(
    int Id,
    int Quantity,
    decimal RefundAmount,
    DateTime ReturnDate,
    int InvoiceId,
    int ProductId,
    string ProductName
);
