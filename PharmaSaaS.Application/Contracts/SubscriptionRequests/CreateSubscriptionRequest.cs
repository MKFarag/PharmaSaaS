namespace PharmaSaaS.Application.Contracts.SubscriptionRequests;

public record CreateSubscriptionRequest(
    string PharmacyName,
    string OwnerName,
    string PhoneNumber,
    string Email
);

#region Validation

public class CreateSubscriptionRequestValidator : AbstractValidator<CreateSubscriptionRequest>
{
    public CreateSubscriptionRequestValidator()
    {
        RuleFor(x => x.PharmacyName)
            .NotEmpty()
            .Length(2, 200);

        RuleFor(x => x.OwnerName)
            .NotEmpty()
            .Length(2, 200);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(RegexPatterns.PhoneNumber)
            .WithMessage("Invalid phone number format.")
            .MaximumLength(20);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);
    }
}

#endregion
