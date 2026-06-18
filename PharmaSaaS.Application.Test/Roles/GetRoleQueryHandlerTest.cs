using PharmaSaaS.Application.Features.Roles.Get;

namespace PharmaSaaS.Application.Test.Roles;

public class GetRoleQueryHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IRoleRepository _roleRepo = A.Fake<IRoleRepository>();
    private readonly GetRoleQueryHandler _handler;

    public GetRoleQueryHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Roles).Returns(_roleRepo);
        _handler = new GetRoleQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var role = new Role { Id = Guid.CreateVersion7().ToString() };

        A.CallTo(() => _roleRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Role?)null);

        var query = new GetRoleQuery(role.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(RoleErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_ValidId_ReturnsRole()
    {
        // Arrange
        var roleId = Guid.CreateVersion7().ToString();
        var role = Role.Create("role-name");
        var permissions = new List<string> { Permissions.ReadRole };

        A.CallTo(() => _roleRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(role);

        A.CallTo(() => _roleRepo.GetClaimsAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(permissions);

        var query = new GetRoleQuery(roleId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(role.Name, result.Value.Name);
        Assert.Equal(permissions, result.Value.Permissions);
    }
}
