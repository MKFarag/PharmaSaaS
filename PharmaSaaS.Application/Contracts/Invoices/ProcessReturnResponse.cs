namespace PharmaSaaS.Application.Contracts.Invoices;

public record ProcessReturnResponse(
    int ReturnInvoiceId,
    decimal TotalRefund
);
