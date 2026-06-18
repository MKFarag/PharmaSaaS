namespace PharmaSaaS.Application.Features.Users.GetAll;

public class GetAllUsersQueryHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ICurrentTenantGetter currentTenantGetter) : IRequestHandler<GetAllUsersQuery, IEnumerable<UserResponse>>
{
    public async Task<IEnumerable<UserResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken = default)
    {
        var tenantId = currentTenantGetter.GetTenantId();

        var users = await cacheService
            .GetOrCreateAsync
            (
                Cache.Keys.UsersWithoutDefaultRole(tenantId!),
                async ct => await unitOfWork.Users.GetAllProjectionWithRolesAsync<UserResponse>(false, ct),
                Cache.Expirations.VeryLong,
                [Cache.Tags.Users(tenantId!)],
                cancellationToken
            );

        return users;
    }
}
