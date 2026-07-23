namespace PharmaSaaS.Application.Contracts.Loyalty;

public record LoyaltyTierResponse(
    int Id,
    string Name,
    decimal SpendThreshold,
    IEnumerable<DiscountRuleResponse> DiscountRules
);
