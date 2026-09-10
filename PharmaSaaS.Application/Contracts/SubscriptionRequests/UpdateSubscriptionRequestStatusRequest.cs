namespace PharmaSaaS.Application.Contracts.SubscriptionRequests;

public record UpdateSubscriptionRequestStatusRequest(
    SubscriptionRequestStatus Status,
    int? DurationInMonths
);

#region Validation

public class UpdateSubscriptionRequestStatusRequestValidator : AbstractValidator<UpdateSubscriptionRequestStatusRequest>
{
    public UpdateSubscriptionRequestStatusRequestValidator()
    {
        RuleFor(s => s.Status)
            .IsInEnum()
            .WithMessage("Invalid status provided.");

        RuleFor(s => s.DurationInMonths)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .GreaterThan(0)
            .When(s => s.Status == SubscriptionRequestStatus.Approved)
            .WithMessage("Duration in months is required and must be greater than zero when approving.");
    }
}

#endregion
