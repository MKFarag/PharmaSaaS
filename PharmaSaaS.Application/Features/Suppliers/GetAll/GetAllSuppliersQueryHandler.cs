namespace PharmaSaaS.Application.Features.Suppliers.GetAll;

public sealed class GetAllSuppliersQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter tenantGetter) : IRequestHandler<GetAllSuppliersQuery, IEnumerable<SupplierResponse>>
{
    public async Task<IEnumerable<SupplierResponse>> Handle(GetAllSuppliersQuery request, CancellationToken cancellationToken = default)
    {
        var tenantId = tenantGetter.GetTenantId();

        return await cacheService
            .GetOrCreateAsync
            (
                Cache.Keys.AllSuppliers(tenantId!),
                unitOfWork.Suppliers.GetAllProjectionAsync<SupplierResponse>,
                Cache.Expirations.VeryLong,
                [Cache.Tags.Suppliers(tenantId!)],
                cancellationToken
            );
    }
}
