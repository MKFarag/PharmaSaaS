namespace PharmaSaaS.Application.Features.Loyalty.UpdateRule;

public record UpdateDiscountRuleCommand(int Id, DiscountRuleRequest Request) : IRequest<Result>;
