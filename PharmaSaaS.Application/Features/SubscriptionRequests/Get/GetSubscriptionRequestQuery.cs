namespace PharmaSaaS.Application.Features.SubscriptionRequests.Get;

public record GetSubscriptionRequestQuery(string Id) : IRequest<Result<SubscriptionRequestResponse>>;
