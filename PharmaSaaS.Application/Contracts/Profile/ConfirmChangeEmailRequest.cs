namespace PharmaSaaS.Application.Contracts.Profile;

public record ConfirmChangeEmailRequest(
    string NewEmail,
    string Token
);

#region Validation

public class ConfirmChangeEmailRequestValidator : AbstractValidator<ConfirmChangeEmailRequest>
{
    public ConfirmChangeEmailRequestValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Token)
            .NotEmpty();
    }
}

#endregion
