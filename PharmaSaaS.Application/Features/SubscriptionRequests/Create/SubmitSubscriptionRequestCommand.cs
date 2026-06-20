namespace PharmaSaaS.Application.Features.SubscriptionRequests.Create;

public record SubmitSubscriptionRequestCommand(CreateSubscriptionRequest Request) : IRequest<Result>;
