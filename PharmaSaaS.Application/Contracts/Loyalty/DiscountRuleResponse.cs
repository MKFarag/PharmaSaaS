namespace PharmaSaaS.Application.Contracts.Loyalty;

public record DiscountRuleResponse(
    int Id,
    decimal MinInvoiceAmount,
    decimal DiscountPercent
);
