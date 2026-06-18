using PharmaSaaS.Application.Contracts.Roles;
using PharmaSaaS.Application.Features.Roles.Add;

namespace PharmaSaaS.Application.Test.Roles;

public class AddRoleCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IRoleRepository _roleRepo = A.Fake<IRoleRepository>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly AddRoleCommandHandler _handler;

    public AddRoleCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Roles).Returns(_roleRepo);
        _handler = new AddRoleCommandHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_WhenRoleNameExists_ReturnsDuplicatedNameError()
    {
        // Arrange
        var roleRequest = new RoleRequest("ExistRole", [Permissions.ReadRole]);

        A.CallTo(() => _roleRepo.NameExistsAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var command = new AddRoleCommand(roleRequest);

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
        var roleRequest = new RoleRequest("Role", [Permissions.ReadRole, "invalid:permission"]);

        A.CallTo(() => _roleRepo.NameExistsAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        var command = new AddRoleCommand(roleRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(RoleErrors.InvalidPermissions, result.Error);
    }

    [Fact]
    public async Task Handle_ValidRoleData_ReturnsSuccess()
    {
        // Arrange
        var permissions = new List<string> { Permissions.ReadRole, Permissions.AddRole };
        var roleRequest = new RoleRequest("Role", permissions);

        A.CallTo(() => _roleRepo.NameExistsAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _unitOfWork.BeginTransactionAsync(A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _roleRepo.CreateAsync(A<Role>.Ignored))
            .Returns(Result.Success());

        A.CallTo(() => _roleRepo.AddClaimsAsync(A<string>.Ignored, A<string>.Ignored, A<IEnumerable<string>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _unitOfWork.CommitTransactionAsync(A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        var command = new AddRoleCommand(roleRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(roleRequest.Name, result.Value.Name);
        Assert.Equal(roleRequest.Permissions, result.Value.Permissions);

        A.CallTo(() => _roleRepo.CreateAsync(A<Role>.Ignored))
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
    public async Task Handle_WhenCreateFails_RollsBackAndReturnsFailure()
    {
        // Arrange
        var roleRequest = new RoleRequest("Role", [Permissions.ReadRole]);

        A.CallTo(() => _roleRepo.NameExistsAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _unitOfWork.BeginTransactionAsync(A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _roleRepo.CreateAsync(A<Role>.Ignored))
            .Returns(Result.Failure(RoleErrors.DuplicatedName));

        A.CallTo(() => _unitOfWork.RollbackTransactionAsync())
            .Returns(Task.CompletedTask);

        var command = new AddRoleCommand(roleRequest);

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
        var roleRequest = new RoleRequest("Role", [Permissions.ReadRole]);

        A.CallTo(() => _roleRepo.NameExistsAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _unitOfWork.BeginTransactionAsync(A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        A.CallTo(() => _roleRepo.CreateAsync(A<Role>.Ignored))
            .ThrowsAsync(new OperationCanceledException());

        A.CallTo(() => _unitOfWork.RollbackTransactionAsync())
            .Returns(Task.CompletedTask);

        var command = new AddRoleCommand(roleRequest);

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
