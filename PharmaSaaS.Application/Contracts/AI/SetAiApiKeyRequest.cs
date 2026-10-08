namespace PharmaSaaS.Application.Contracts.AI;

public record SetAiApiKeyRequest(string? ApiKey);

#region Validation

public class SetAiApiKeyRequestValidator : AbstractValidator<SetAiApiKeyRequest>
{
    public SetAiApiKeyRequestValidator()
    {
        RuleFor(x => x.ApiKey)
            .MaximumLength(200)
            .When(x => x.ApiKey is not null);
    }
}

#endregion
