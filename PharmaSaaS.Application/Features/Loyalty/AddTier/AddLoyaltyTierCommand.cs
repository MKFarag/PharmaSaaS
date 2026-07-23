namespace PharmaSaaS.Application.Features.Loyalty.AddTier;

public record AddLoyaltyTierCommand(LoyaltyTierRequest Request) : IRequest<Result<LoyaltyTierResponse>>;
