namespace PharmaSaaS.Application.Features.SubscriptionRequests.GetAll;

public record GetAllSubscriptionRequestsQuery(string? Status = null) : IRequest<IEnumerable<SubscriptionRequestResponse>>;
