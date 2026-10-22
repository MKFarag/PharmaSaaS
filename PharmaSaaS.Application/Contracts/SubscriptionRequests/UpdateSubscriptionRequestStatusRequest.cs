namespace PharmaSaaS.Application.Contracts.SubscriptionRequests;

public record UpdateSubscriptionRequestStatusRequest(
    string Status,
    int? DurationInMonths
);

#region Validation

public class UpdateSubscriptionRequestStatusRequestValidator : AbstractValidator<UpdateSubscriptionRequestStatusRequest>
{
    public UpdateSubscriptionRequestStatusRequestValidator()
    {
        RuleFor(s => s.Status)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Status is required.")
            .IsEnumName(typeof(SubscriptionRequestStatus), caseSensitive: false)
            .WithMessage("Invalid status provided. Allowed statuses are: Approved, Rejected, or Contacted.")
            .Must(s => !string.Equals(s, nameof(SubscriptionRequestStatus.Pending), StringComparison.OrdinalIgnoreCase))
            .WithMessage("Status cannot be set to Pending. A subscription request can only be updated to: Approved, Rejected, or Contacted.");

        RuleFor(s => s.DurationInMonths)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Duration in months is required and must be greater than zero when approving.")
            .GreaterThan(0)
            .WithMessage("Duration in months is required and must be greater than zero when approving.")
            .When(s => string.Equals(s.Status, nameof(SubscriptionRequestStatus.Approved), StringComparison.OrdinalIgnoreCase));
    }
}

#endregion
