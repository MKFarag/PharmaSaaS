namespace PharmaSaaS.Application.Contracts.Auth;

public record ConfirmEmailRequest(
    string UserId,
    string Token
);

#region Validation

public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Token)
            .NotEmpty();
    }
}

#endregion
