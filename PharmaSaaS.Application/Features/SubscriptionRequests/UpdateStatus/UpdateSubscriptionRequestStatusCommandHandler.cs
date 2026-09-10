namespace PharmaSaaS.Application.Features.SubscriptionRequests.UpdateStatus;

public sealed class UpdateSubscriptionRequestStatusCommandHandler(IUnitOfWork unitOfWork, INotificationService notificationService)
    : IRequestHandler<UpdateSubscriptionRequestStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateSubscriptionRequestStatusCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.SubscriptionRequests.GetAsync(command.Id, cancellationToken) is not { } subscriptionRequest)
            return Result.Failure(SubscriptionRequestErrors.NotFound);

        DateOnly? subscriptionEnd = null;

        switch (command.Request.Status)
        {
            case SubscriptionRequestStatus.Approved:
                subscriptionRequest.Approve();
                subscriptionEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(command.Request.DurationInMonths!.Value));
                var pharmacy = Pharmacy.Create(subscriptionRequest.PharmacyName);
                pharmacy.SetSubscriptionEnd(subscriptionEnd.Value);
                await unitOfWork.Pharmacies.AddAsync(pharmacy, cancellationToken);
                break;

            case SubscriptionRequestStatus.Rejected:
                subscriptionRequest.Reject();
                break;

            case SubscriptionRequestStatus.Contacted:
                subscriptionRequest.Contact();
                break;
        }

        await unitOfWork.SharedCompleteAsync(cancellationToken);

        await notificationService
            .SendSubscriptionRequestStatusUpdateAsync
            (
                subscriptionRequest.Email,
                subscriptionRequest.PharmacyName,
                command.Request.Status.ToString(),
                subscriptionEnd
            );

        return Result.Success();
    }
}
