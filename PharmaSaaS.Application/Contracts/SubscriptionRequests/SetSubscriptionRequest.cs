namespace PharmaSaaS.Application.Contracts.SubscriptionRequests;

public record SetSubscriptionRequest(
    DateOnly End
);

#region Validation

public class SetSubscriptionRequestValidator : AbstractValidator<SetSubscriptionRequest>
{
    public SetSubscriptionRequestValidator()
    {
        RuleFor(x => x.End)
            .NotEmpty()
            .Must(x => x > DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("End date must be in the future");
    }
}

#endregion