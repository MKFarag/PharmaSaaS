namespace PharmaSaaS.Application.Contracts.Invoices;

public record ProcessReturnResponse(
    decimal TotalRefund,
    decimal DiscountClawback
);
