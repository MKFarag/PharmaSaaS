namespace PharmaSaaS.Application.Features.SubscriptionRequests.GetAll;

public record GetAllSubscriptionRequestsQuery(SubscriptionRequestStatus? Status = null) : IRequest<IEnumerable<SubscriptionRequestResponse>>;
