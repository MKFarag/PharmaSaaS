namespace PharmaSaaS.Application.Contracts.Profile;

public record ChangeEmailRequest(
    string NewEmail
);

#region Validation

public class ChangeEmailRequestValidator : AbstractValidator<ChangeEmailRequest>
{
    public ChangeEmailRequestValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .EmailAddress();
    }
}

#endregion
