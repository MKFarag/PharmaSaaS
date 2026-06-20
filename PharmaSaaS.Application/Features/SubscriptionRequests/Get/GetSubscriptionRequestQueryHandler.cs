namespace PharmaSaaS.Application.Features.SubscriptionRequests.Get;

public class GetSubscriptionRequestQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetSubscriptionRequestQuery, Result<SubscriptionRequestResponse>>
{
    public async Task<Result<SubscriptionRequestResponse>> Handle(GetSubscriptionRequestQuery request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.SubscriptionRequests.GetAsync(request.Id, cancellationToken) is not { } subscriptionRequest)
            return Result.Failure<SubscriptionRequestResponse>(SubscriptionRequestErrors.NotFound);

        return Result.Success(subscriptionRequest.Adapt<SubscriptionRequestResponse>());
    }
}
