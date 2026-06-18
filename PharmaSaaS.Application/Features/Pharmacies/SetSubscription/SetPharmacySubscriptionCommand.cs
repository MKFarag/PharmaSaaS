namespace PharmaSaaS.Application.Features.Pharmacies.SetSubscription;

public record SetPharmacySubscriptionCommand(int Id, SetSubscriptionRequest Request) : IRequest<Result>;
