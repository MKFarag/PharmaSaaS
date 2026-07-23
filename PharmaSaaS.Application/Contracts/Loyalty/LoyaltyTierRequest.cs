namespace PharmaSaaS.Application.Contracts.Loyalty;

public record LoyaltyTierRequest(
    string Name,
    decimal SpendThreshold,
    IEnumerable<DiscountRuleRequest>? DiscountRules = null
);

public class LoyaltyTierRequestValidator : AbstractValidator<LoyaltyTierRequest>
{
    public LoyaltyTierRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.SpendThreshold)
            .GreaterThanOrEqualTo(0);
    }
}
