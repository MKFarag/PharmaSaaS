namespace PharmaSaaS.Application.Contracts.Payments;

public record VerifySubscriptionPaymentRequest(
    int ExtendByMonths,
    decimal? Amount
);

#region Validation

public class VerifySubscriptionPaymentRequestValidator : AbstractValidator<VerifySubscriptionPaymentRequest>
{
    public VerifySubscriptionPaymentRequestValidator()
    {
        RuleFor(x => x.ExtendByMonths)
            .GreaterThan(0)
            .WithMessage("Extend by months must be at least 1.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .When(x => x.Amount.HasValue);
    }
}

#endregion
