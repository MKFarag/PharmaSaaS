namespace PharmaSaaS.Application.Contracts.Invoices;

public record InvoiceResponse(
    int Id,
    int ShiftId,
    int? CustomerId,
    string? CustomerName,
    decimal DiscountAmount,
    decimal FinalAmount,
    bool HasReturn,
    DateTime CreatedAt
);
