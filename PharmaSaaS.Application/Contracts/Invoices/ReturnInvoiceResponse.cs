namespace PharmaSaaS.Application.Contracts.Invoices;

public record ReturnInvoiceResponse(
    int Id,
    int OriginalInvoiceId,
    int ShiftId,
    decimal TotalRefund,
    DateTimeOffset CreatedAt
);
