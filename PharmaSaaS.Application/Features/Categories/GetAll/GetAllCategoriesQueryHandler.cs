namespace PharmaSaaS.Application.Features.Categories.GetAll;

public class GetAllCategoriesQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryResponse>>
{
    public async Task<IEnumerable<CategoryResponse>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken = default)
    {
        var tenantId = currentTenantGetter.GetTenantId();

        return await cacheService
            .GetOrCreateAsync
            (
                Cache.Keys.AllCategories(tenantId!),
                unitOfWork.Categories.GetAllProjectionAsync<CategoryResponse>,
                Cache.Expirations.VeryLong,
                [Cache.Tags.Categories(tenantId!)],
                cancellationToken
            );
    }
}
