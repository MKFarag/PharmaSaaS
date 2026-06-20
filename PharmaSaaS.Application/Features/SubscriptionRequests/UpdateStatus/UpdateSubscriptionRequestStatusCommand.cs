namespace PharmaSaaS.Application.Features.SubscriptionRequests.UpdateStatus;

public record UpdateSubscriptionRequestStatusCommand(string Id, UpdateSubscriptionRequestStatusRequest Request) : IRequest<Result>;
