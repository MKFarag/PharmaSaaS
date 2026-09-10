namespace PharmaSaaS.Application.Contracts.Invoices;

public record InvoiceDetailResponse(
    int Id,
    IList<InvoiceItemResponse> Items,
    int? CustomerId,
    string? CustomerName,
    decimal Total,
    decimal DiscountAmount,
    decimal FinalAmount,
    bool HasReturn,
    IList<ReturnResponse> Returns,
    DateTime CreatedAt,
    ShiftResponse Shift
);
