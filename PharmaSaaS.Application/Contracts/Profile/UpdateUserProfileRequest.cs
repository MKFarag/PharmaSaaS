namespace PharmaSaaS.Application.Contracts.Profile;

public record UpdateUserProfileRequest(
    string FirstName,
    string LastName
);

#region Validation

public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Matches(RegexPatterns.PersonName)
            .WithMessage("FirstName must start and end with a letter, and spaces, hyphens, and apostrophes are allowed only in the middle.")
            .Length(2, 50);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .Matches(RegexPatterns.PersonName)
            .WithMessage("LastName must start and end with a letter, and spaces, hyphens, and apostrophes are allowed only in the middle.")
            .Length(2, 100);
    }
}

#endregion