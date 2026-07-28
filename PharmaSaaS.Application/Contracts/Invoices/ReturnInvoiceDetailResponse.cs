namespace PharmaSaaS.Application.Contracts.Invoices;

public record ReturnInvoiceDetailResponse(
    int Id,
    int OriginalInvoiceId,
    int ShiftId,
    decimal TotalRefund,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<ReturnInvoiceItemResponse> Items
);
