namespace PharmaSaaS.Application.Features.Loyalty.DeleteRule;

public sealed class DeleteDiscountRuleCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter tenantGetter) : IRequestHandler<DeleteDiscountRuleCommand, Result>
{
    public async Task<Result> Handle(DeleteDiscountRuleCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.DiscountRules.GetAsync([command.Id], cancellationToken: cancellationToken) is not { } rule)
            return Result.Failure(LoyaltyErrors.RuleNotFound);

        unitOfWork.DiscountRules.Delete(rule);
        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveByTagAsync(Cache.Tags.LoyaltyTiers(tenantGetter.GetTenantId()!), cancellationToken);

        return Result.Success();
    }
}
