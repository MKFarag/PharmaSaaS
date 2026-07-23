namespace PharmaSaaS.Application.Features.Loyalty.DeleteRule;

public record DeleteDiscountRuleCommand(int Id) : IRequest<Result>;
