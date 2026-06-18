namespace PharmaSaaS.Application.Contracts.Auth;

public record ResendConfirmationEmailRequest(
    string Email
);

#region Validator

public class ResendConfirmationEmailRequestValidator : AbstractValidator<ResendConfirmationEmailRequest>
{
    public ResendConfirmationEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}

#endregion