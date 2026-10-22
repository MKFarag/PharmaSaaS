namespace PharmaSaaS.Application.Features.Pharmacies.SetSubscription;

public record SetPharmacySubscriptionCommand(string Id, SetSubscriptionRequest Request) : IRequest<Result>;
