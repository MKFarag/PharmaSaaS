namespace PharmaSaaS.Application.Features.Roles.GetAll;

public sealed class GetAllRolesQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<GetAllRolesQuery, IEnumerable<RoleResponse>>
{
    public async Task<IEnumerable<RoleResponse>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken = default)
    {
        var tenantId = currentTenantGetter.GetTenantId();

        var roles = await cacheService
            .GetOrCreateAsync
            (
                Cache.Keys.RolesWithoutDefault(tenantId!),
                async ct => await unitOfWork.Roles.GetAllAsync(false, true, ct),
                Cache.Expirations.VeryLong,
                [Cache.Tags.Roles(tenantId!)],
                cancellationToken
            );

        var response = roles
            .Where(r => request.IncludeDisabled || !r.IsDisabled)
            .Select(r => new RoleResponse(r.Id, r.Name, r.IsDisabled))
            .ToList();

        return response;
    }
}
