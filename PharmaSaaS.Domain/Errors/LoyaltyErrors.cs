namespace PharmaSaaS.Domain.Errors;

public record LoyaltyErrors
{
    public static readonly Error TierNotFound =
        new("LoyaltyTier.NotFound", "No loyalty tier found", StatusCodes.NotFound);

    public static readonly Error DuplicatedTierName =
        new("LoyaltyTier.DuplicatedName", "Another loyalty tier with the same name already exists", StatusCodes.Conflict);

    public static readonly Error TierInUse =
        new("LoyaltyTier.InUse", "Cannot delete tier because it is associated with one or more customers", StatusCodes.Conflict);

    public static readonly Error RuleNotFound =
        new("DiscountRule.NotFound", "No discount rule found", StatusCodes.NotFound);

    public static readonly Error DuplicatedRuleAmount =
        new("DiscountRule.DuplicatedAmount", "Another discount rule with the same minimum invoice amount already exists for this tier", StatusCodes.Conflict);
}
