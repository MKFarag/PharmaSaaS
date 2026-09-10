namespace PharmaSaaS.Application.Features.SubscriptionRequests.Create;

public sealed class SubmitSubscriptionRequestCommandHandler(IUnitOfWork unitOfWork, INotificationService notificationService) : IRequestHandler<SubmitSubscriptionRequestCommand, Result>
{
    public async Task<Result> Handle(SubmitSubscriptionRequestCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.SubscriptionRequests
            .ExistsPendingAsync
            (
                command.Request.PharmacyName,
                command.Request.PhoneNumber,
                command.Request.Email,
                cancellationToken
            ))
            return Result.Failure(SubscriptionRequestErrors.AlreadyPending);

        var subscriptionRequest = SubscriptionRequest
            .Create
            (
                command.Request.PharmacyName,
                command.Request.OwnerName,
                command.Request.PhoneNumber,
                command.Request.Email
            );

        await unitOfWork.SubscriptionRequests.AddAsync(subscriptionRequest, cancellationToken);

        await unitOfWork.SharedCompleteAsync(cancellationToken);

        await notificationService
            .SendNewSubscriptionRequestNotificationAsync
            (
                subscriptionRequest.PharmacyName,
                subscriptionRequest.OwnerName,
                subscriptionRequest.PhoneNumber,
                subscriptionRequest.Email,
                subscriptionRequest.CreatedAt
            );

        return Result.Success();
    }
}
