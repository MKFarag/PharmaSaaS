namespace PharmaSaaS.Application.Features.Loyalty.DeleteRule;

public sealed class DeleteDiscountRuleCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter tenantGetter) : IRequestHandler<DeleteDiscountRuleCommand, Result>
{
    public async Task<Result> Handle(DeleteDiscountRuleCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.DiscountRules.GetAsync([command.Id], cancellationToken) is not { } rule)
            return Result.Failure(LoyaltyErrors.RuleNotFound);

        unitOfWork.DiscountRules.Delete(rule);

        await cacheService.RemoveByTagAsync(Cache.Tags.LoyaltyTiers(tenantGetter.GetTenantId()!), cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
