namespace PharmaSaaS.Application.Features.SubscriptionRequests.GetAll;

public class GetAllSubscriptionRequestsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllSubscriptionRequestsQuery, IEnumerable<SubscriptionRequestResponse>>
{
    public async Task<IEnumerable<SubscriptionRequestResponse>> Handle(GetAllSubscriptionRequestsQuery request, CancellationToken cancellationToken = default)
        => await unitOfWork.SubscriptionRequests.GetAllAsync<SubscriptionRequestResponse>(request.Status, cancellationToken);
}
