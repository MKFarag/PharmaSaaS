namespace PharmaSaaS.Application.Features.Roles.Get;

public sealed class GetRoleQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetRoleQuery, Result<RoleDetailResponse>>
{
    public async Task<Result<RoleDetailResponse>> Handle(GetRoleQuery request, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Roles.GetAsync(request.Id, cancellationToken) is not { } role)
            return Result.Failure<RoleDetailResponse>(RoleErrors.NotFound);

        var permissions = await unitOfWork.Roles.GetClaimsAsync(role.Id, Permissions.Type, cancellationToken);

        var response = new RoleDetailResponse(role.Id, role.Name, role.IsDisabled, permissions);

        return Result.Success(response);
    }
}
