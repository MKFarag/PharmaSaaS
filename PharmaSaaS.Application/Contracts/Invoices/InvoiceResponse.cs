namespace PharmaSaaS.Application.Contracts.Invoices;

public record InvoiceResponse(
    int Id,
    int ShiftId,
    int? CustomerId,
    string? CustomerName,
    decimal Total,
    decimal DiscountAmount,
    decimal FinalAmount,
    int ItemCount,
    bool HasReturn,
    DateOnly Date
);
