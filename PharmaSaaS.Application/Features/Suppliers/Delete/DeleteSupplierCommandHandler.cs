namespace PharmaSaaS.Application.Features.Suppliers.Delete;

public sealed class DeleteSupplierCommandHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter tenantGetter) : IRequestHandler<DeleteSupplierCommand, Result>
{
    public async Task<Result> Handle(DeleteSupplierCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Suppliers.GetAsync([command.Id], cancellationToken) is not { } supplier)
            return Result.Failure(SupplierErrors.NotFound);

        if (await unitOfWork.PurchaseOrders.AnyAsync(o => o.SupplierId == command.Id, cancellationToken))
            return Result.Failure(SupplierErrors.HasOrders);

        unitOfWork.Suppliers.Delete(supplier);

        await cacheService.RemoveByTagAsync(Cache.Tags.Suppliers(tenantGetter.GetTenantId()!), cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
