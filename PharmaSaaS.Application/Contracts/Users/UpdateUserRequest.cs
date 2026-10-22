namespace PharmaSaaS.Application.Contracts.Users;

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    IList<string> Roles
);

#region Validation

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
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

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Roles)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Roles)
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("You cannot add duplicated role for the same user")
            .When(x => x.Roles != null);
    }
}

#endregion
