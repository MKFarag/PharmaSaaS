namespace PharmaSaaS.Application.Features.Loyalty.AddRule;

public sealed class AddDiscountRuleCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter tenantGetter) : IRequestHandler<AddDiscountRuleCommand, Result<DiscountRuleResponse>>
{
    public async Task<Result<DiscountRuleResponse>> Handle(AddDiscountRuleCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.LoyaltyTiers.GetAsync([command.TierId], cancellationToken: cancellationToken) is null)
            return Result.Failure<DiscountRuleResponse>(LoyaltyErrors.TierNotFound);

        if (await unitOfWork.DiscountRules.AnyAsync(r => r.LoyaltyTierId == command.TierId && r.MinInvoiceAmount == command.Request.MinInvoiceAmount, cancellationToken))
            return Result.Failure<DiscountRuleResponse>(LoyaltyErrors.DuplicatedRuleAmount);

        var rule = DiscountRule.Create(command.TierId, command.Request.MinInvoiceAmount, command.Request.DiscountPercent);

        await unitOfWork.DiscountRules.AddAsync(rule, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveByTagAsync(Cache.Tags.LoyaltyTiers(tenantGetter.GetTenantId()!), cancellationToken);

        return Result.Success(rule.Adapt<DiscountRuleResponse>());
    }
}
