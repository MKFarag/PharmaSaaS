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
            .Matches(RegexPatterns.BusinessName)
            .WithMessage("Pharmacy name must start and end with a letter or number, and can contain spaces, hyphens, apostrophes, periods, underscores, and ampersands.")
            .Length(2, 200);

        RuleFor(x => x.OwnerName)
            .NotEmpty()
            .Matches(RegexPatterns.PersonName)
            .WithMessage("Owner name must start and end with a letter, and spaces, hyphens, and apostrophes are allowed only in the middle.")
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
