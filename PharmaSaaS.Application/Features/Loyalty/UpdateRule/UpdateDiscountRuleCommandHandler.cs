namespace PharmaSaaS.Application.Features.Loyalty.UpdateRule;

public sealed class UpdateDiscountRuleCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter tenantGetter) : IRequestHandler<UpdateDiscountRuleCommand, Result>
{
    public async Task<Result> Handle(UpdateDiscountRuleCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.DiscountRules.GetAsync([command.Id], cancellationToken) is not { } rule)
            return Result.Failure(LoyaltyErrors.RuleNotFound);

        if (rule.MinInvoiceAmount != command.Request.MinInvoiceAmount &&
            await unitOfWork.DiscountRules
            .AnyAsync
            (
                r => r.LoyaltyTierId == rule.LoyaltyTierId && r.MinInvoiceAmount == command.Request.MinInvoiceAmount,
                cancellationToken
            ))
            return Result.Failure(LoyaltyErrors.DuplicatedRuleAmount);

        rule.Update(command.Request.MinInvoiceAmount, command.Request.DiscountPercent);

        unitOfWork.DiscountRules.Update(rule);

        await cacheService.RemoveByTagAsync(Cache.Tags.LoyaltyTiers(tenantGetter.GetTenantId()!), cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
