namespace PharmaSaaS.Application.Contracts.Auth;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName
);

#region Validation

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .Matches(RegexPatterns.Password)
            .WithMessage("Password must be at least 8 characters and must contains digit, Lowercase, Uppercase and NonAlphanumeric");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Matches(RegexPatterns.OnlyLettersWithSpaces)
            .WithMessage("FirstName must start and end with a letter, and spaces are allowed only in the middle.")
            .Length(2, 50);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .Matches(RegexPatterns.OnlyLettersWithSpaces)
            .WithMessage("LastName must start and end with a letter, and spaces are allowed only in the middle.")
            .Length(2, 100);
    }
}

#endregion
