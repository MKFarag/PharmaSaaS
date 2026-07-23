namespace PharmaSaaS.Application.Features.Loyalty.AddRule;

public record AddDiscountRuleCommand(int TierId, DiscountRuleRequest Request) : IRequest<Result<DiscountRuleResponse>>;
