using PharmaSaaS.Application.Features.Roles.ToggleStatus;

namespace PharmaSaaS.Application.Test.Roles;

public class ToggleRoleStatusCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IRoleRepository _roleRepo = A.Fake<IRoleRepository>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly ToggleRoleStatusCommandHandler _handler;

    public ToggleRoleStatusCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Roles).Returns(_roleRepo);
        _handler = new ToggleRoleStatusCommandHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var role = new Role { Id = Guid.CreateVersion7().ToString() };

        A.CallTo(() => _roleRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Role?)null);

        var command = new ToggleRoleStatusCommand(role.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(RoleErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_ValidId_ReturnsSuccess()
    {
        // Arrange
        var role = new Role { Id = Guid.CreateVersion7().ToString() };

        A.CallTo(() => _roleRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(role);

        A.CallTo(() => _roleRepo.UpdateAsync(A<Role>.Ignored))
            .Returns(Result.Success());

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        var command = new ToggleRoleStatusCommand(role.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        A.CallTo(() => _roleRepo.UpdateAsync(A<Role>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
