namespace PharmaSaaS.Application.Contracts.Loyalty;

public record LoyaltyTierRequest(
    string Name,
    decimal SpendThreshold,
    IEnumerable<DiscountRuleRequest>? DiscountRules
);

#region Validation

public class LoyaltyTierRequestValidator : AbstractValidator<LoyaltyTierRequest>
{
    public LoyaltyTierRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Matches(RegexPatterns.BusinessName)
            .WithMessage("Loyalty tier name must start and end with a letter or number, and can contain spaces, hyphens, apostrophes, periods, underscores, and ampersands.")
            .MaximumLength(100);

        RuleFor(x => x.SpendThreshold)
            .GreaterThanOrEqualTo(0);
    }
}

#endregion
