using PharmaSaaS.Application.Interfaces.Infrastructure;

namespace PharmaSaaS.Application.Features.Suppliers.ResetBalance;

public sealed class ResetSupplierBalanceCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter tenantGetter)
    : IRequestHandler<ResetSupplierBalanceCommand, Result>
{
    public async Task<Result> Handle(ResetSupplierBalanceCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Suppliers.GetAsync([command.Id], cancellationToken) is not { } supplier)
            return Result.Failure(SupplierErrors.NotFound);

        supplier.ResetBalance();

        await unitOfWork.CompleteAsync(cancellationToken);

        await cacheService.RemoveByTagAsync(Cache.Tags.Suppliers(tenantGetter.GetTenantId()!), cancellationToken);

        return Result.Success();
    }
}
