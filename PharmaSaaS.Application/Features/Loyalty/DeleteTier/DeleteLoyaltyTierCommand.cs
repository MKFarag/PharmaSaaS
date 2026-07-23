namespace PharmaSaaS.Application.Features.Loyalty.DeleteTier;

public record DeleteLoyaltyTierCommand(int Id) : IRequest<Result>;
