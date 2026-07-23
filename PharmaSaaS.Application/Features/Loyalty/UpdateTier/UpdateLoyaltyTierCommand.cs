namespace PharmaSaaS.Application.Features.Loyalty.UpdateTier;

public record UpdateLoyaltyTierCommand(int Id, LoyaltyTierRequest Request) : IRequest<Result>;
