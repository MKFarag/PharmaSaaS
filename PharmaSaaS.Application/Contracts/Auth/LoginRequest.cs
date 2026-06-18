namespace PharmaSaaS.Application.Contracts.Auth;

public record LoginRequest(
    string Email,
    string Password
);

#region Validation

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}

#endregion
