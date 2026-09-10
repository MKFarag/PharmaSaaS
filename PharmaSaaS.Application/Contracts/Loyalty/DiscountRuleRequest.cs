namespace PharmaSaaS.Application.Contracts.Loyalty;

public record DiscountRuleRequest(
    decimal MinInvoiceAmount,
    decimal DiscountPercent
);

#region Validation

public class DiscountRuleRequestValidator : AbstractValidator<DiscountRuleRequest>
{
    public DiscountRuleRequestValidator()
    {
        RuleFor(x => x.MinInvoiceAmount)
            .GreaterThan(0);

        RuleFor(x => x.DiscountPercent)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}

#endregion
