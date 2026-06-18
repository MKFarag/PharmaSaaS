using PharmaSaaS.Application.Contracts.Roles;
using PharmaSaaS.Application.Features.Roles.Update;

namespace PharmaSaaS.Application.Test.Roles;

public class UpdateRoleCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IRoleRepository _roleRepo = A.Fake<IRoleRepository>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly UpdateRoleCommandHandler _handler;

    public UpdateRoleCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Roles).Returns(_roleRepo);
        _handler = new UpdateRoleCommandHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var roleId = Guid.CreateVersion7().ToString();
        var roleRequest = new RoleRequest("Role", [Permissions.ReadRole]);

        A.CallTo(() => _roleRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Role?)null);

        var command = new UpdateRoleCommand(roleId, roleRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(RoleErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenRoleNameExists_ReturnsDuplicatedNameError()
    {
        // Arrange
        var roleId = Guid.CreateVersion7().ToString();
        var roleRequest = new RoleRequest("ExistRole", [Permissions.ReadRole]);
        var role = Role.Create("Role");

        A.CallTo(() => _roleRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(role);

        A.CallTo(() => _roleRepo.NameExistsAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var command = new UpdateRoleCommand(roleId, roleRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(RoleErrors.DuplicatedName, result.Error);
    }

    [Fact]
    public async Task Handle_WhenPassInvalidPermission_ReturnsInvalidPermissionsError()
    {
        // Arrange
        var roleId = Guid.CreateVersion7().ToString();
        var roleRequest = new RoleRequest("Role", ["invalid:permission"]);
        var role = Role.Create("Role");

        A.CallTo(() => _roleRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(role);

        A.CallTo(() => _roleRepo.NameExistsAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        var command = new UpdateRoleCommand(roleId, roleRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(RoleErrors.InvalidPermissions, result.Error);
    }

    [Fact]
    public async Task Handle_ValidDataWithNoPermissionChanges_ReturnsSuccessWithNoPermissionChanges()
    {
        // Arrange
        var roleId = Guid.CreateVersion7().ToString();
        var permissions = new List<string> { Permissions.ReadRole };
        var roleRequest = new RoleRequest("Role", permissions);
        var role = Role.Create("Role");

        A.CallTo(() => _roleRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(role);

        A.CallTo(() => _roleRepo.NameExistsAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _unitOfWork.BeginTransactionAsync(A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _roleRepo.UpdateAsync(A<Role>.Ignored))
            .Returns(Result.Success());

        A.CallTo(() => _roleRepo.GetClaimsAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(permissions);

        A.CallTo(() => _unitOfWork.CommitTransactionAsync(A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        var command = new UpdateRoleCommand(roleId, roleRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        A.CallTo(() => _roleRepo.DeleteClaimsAsync(A<string>.Ignored, A<IEnumerable<string>>.Ignored, A<CancellationToken>.Ignored))
            .MustNotHaveHappened();

        A.CallTo(() => _roleRepo.AddClaimsAsync(A<string>.Ignored, A<string>.Ignored, A<IEnumerable<string>>.Ignored, A<CancellationToken>.Ignored))
            .MustNotHaveHappened();

        A.CallTo(() => _unitOfWork.BeginTransactionAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.CommitTransactionAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.RollbackTransactionAsync())
            .MustNotHaveHappened();

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_ValidDataWithPermissionChanges_ReturnsSuccessWithChanges()
    {
        // Arrange
        var roleId = Guid.CreateVersion7().ToString();
        var currentPermissions = new List<string> { Permissions.ReadRole, Permissions.AddRole };
        var newPermissions = new List<string> { Permissions.ReadRole, Permissions.UpdateRole };
        var roleRequest = new RoleRequest("NewRoleName", newPermissions);
        var role = Role.Create("Role");

        A.CallTo(() => _roleRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(role);

        A.CallTo(() => _roleRepo.NameExistsAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _unitOfWork.BeginTransactionAsync(A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _roleRepo.UpdateAsync(A<Role>.Ignored))
            .Returns(Result.Success());

        A.CallTo(() => _roleRepo.GetClaimsAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(currentPermissions);

        A.CallTo(() => _roleRepo.DeleteClaimsAsync(A<string>.Ignored, A<IEnumerable<string>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _roleRepo.AddClaimsAsync(A<string>.Ignored, A<string>.Ignored, A<IEnumerable<string>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _unitOfWork.CommitTransactionAsync(A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        var command = new UpdateRoleCommand(roleId, roleRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        A.CallTo(() => _roleRepo.DeleteClaimsAsync(A<string>.Ignored, A<IEnumerable<string>>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _roleRepo.AddClaimsAsync(A<string>.Ignored, A<string>.Ignored, A<IEnumerable<string>>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.BeginTransactionAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.CommitTransactionAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.RollbackTransactionAsync())
            .MustNotHaveHappened();

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenUpdateFails_RollsBackAndReturnsFailure()
    {
        // Arrange
        var roleId = Guid.CreateVersion7().ToString();
        var roleRequest = new RoleRequest("Role", [Permissions.ReadRole]);
        var role = Role.Create("Role");

        A.CallTo(() => _roleRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(role);

        A.CallTo(() => _roleRepo.NameExistsAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _unitOfWork.BeginTransactionAsync(A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _roleRepo.UpdateAsync(A<Role>.Ignored))
            .Returns(Result.Failure(RoleErrors.DuplicatedName));

        A.CallTo(() => _unitOfWork.RollbackTransactionAsync())
            .Returns(Task.CompletedTask);

        var command = new UpdateRoleCommand(roleId, roleRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(RoleErrors.DuplicatedName, result.Error);

        A.CallTo(() => _unitOfWork.BeginTransactionAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.RollbackTransactionAsync())
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.CommitTransactionAsync(A<CancellationToken>.Ignored))
            .MustNotHaveHappened();

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenOperationCanceled_RollsBackAndReturnsCreationCanceled()
    {
        // Arrange
        var roleId = Guid.CreateVersion7().ToString();
        var roleRequest = new RoleRequest("Role", [Permissions.ReadRole]);
        var role = Role.Create("Role");

        A.CallTo(() => _roleRepo.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(role);

        A.CallTo(() => _roleRepo.NameExistsAsync(A<string>.Ignored, A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _unitOfWork.BeginTransactionAsync(A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _roleRepo.UpdateAsync(A<Role>.Ignored))
            .ThrowsAsync(new OperationCanceledException());

        A.CallTo(() => _unitOfWork.RollbackTransactionAsync())
            .Returns(Task.CompletedTask);

        var command = new UpdateRoleCommand(roleId, roleRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(RoleErrors.CreationCanceled, result.Error);

        A.CallTo(() => _unitOfWork.RollbackTransactionAsync())
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.CommitTransactionAsync(A<CancellationToken>.Ignored))
            .MustNotHaveHappened();

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustNotHaveHappened();
    }
}
