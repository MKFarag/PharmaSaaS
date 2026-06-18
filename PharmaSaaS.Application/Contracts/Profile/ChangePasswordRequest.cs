namespace PharmaSaaS.Application.Contracts.Profile;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

#region Validation

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .Matches(RegexPatterns.Password)
            .WithMessage("Password must be at least 8 characters and must contains digit, Lowercase, Uppercase and NonAlphanumeric")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("New password must be different from the current password.");
    }
}

#endregion
