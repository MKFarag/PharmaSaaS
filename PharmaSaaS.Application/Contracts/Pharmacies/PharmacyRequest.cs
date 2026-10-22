namespace PharmaSaaS.Application.Contracts.Pharmacies;

public record PharmacyRequest(
    string Name
);

#region Validation

public class PharmacyRequestValidator : AbstractValidator<PharmacyRequest>
{
    public PharmacyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Matches(RegexPatterns.BusinessName)
            .WithMessage("Pharmacy name must start and end with a letter or number, and can contain spaces, hyphens, apostrophes, periods, underscores, and ampersands.")
            .Length(2, 200);
    }
}

#endregion
