namespace PharmaSaaS.Application.Contracts.SubscriptionRequests;

public record UpdateSubscriptionRequestStatusRequest(string Status);

#region Validation

public class UpdateSubscriptionRequestStatusRequestValidator : AbstractValidator<UpdateSubscriptionRequestStatusRequest>
{
    public UpdateSubscriptionRequestStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => s is SubscriptionRequestStatus.Approved or SubscriptionRequestStatus.Rejected or SubscriptionRequestStatus.Contacted)
            .WithMessage($"Status must be one of: {SubscriptionRequestStatus.Approved}, {SubscriptionRequestStatus.Rejected}, {SubscriptionRequestStatus.Contacted}");
    }
}

#endregion
