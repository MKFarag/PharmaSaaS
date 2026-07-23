namespace PharmaSaaS.Application.Features.Loyalty.GetAll;

public record GetAllLoyaltyTiersQuery() : IRequest<IEnumerable<LoyaltyTierResponse>>;
