namespace PharmaSaaS.Application.Contracts.Invoices;

public record ReturnInvoiceSummaryResponse(
    int Id,
    decimal TotalRefund,
    IList<ReturnInvoiceItemResponse> Items,
    DateTimeOffset CreatedAt
);
