using PharmaSaaS.Application.Contracts.Users;
using PharmaSaaS.Application.Features.Users.Add;

namespace PharmaSaaS.Application.Test.Users;

public class AddUserCommandHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IUserRepository _userRepo = A.Fake<IUserRepository>();
    private readonly IRoleRepository _roleRepo = A.Fake<IRoleRepository>();
    private readonly ICacheService _cacheService = A.Fake<ICacheService>();
    private readonly ICurrentTenantGetter _currentTenantGetter = A.Fake<ICurrentTenantGetter>();
    private readonly AddUserCommandHandler _handler;

    public AddUserCommandHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Users).Returns(_userRepo);
        A.CallTo(() => _unitOfWork.Roles).Returns(_roleRepo);
        _handler = new AddUserCommandHandler(_unitOfWork, _cacheService, _currentTenantGetter);
    }

    [Fact]
    public async Task Handle_WhenEmailExists_ReturnsDuplicatedEmailError()
    {
        // Arrange
        var userRequest = new CreateUserRequest("First", "Last", "ExistEmail@test.com", "Pass", ["Role"]);

        A.CallTo(() => _userRepo.EmailExistsAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var command = new AddUserCommand(userRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.DuplicatedEmail, result.Error);
    }

    [Fact]
    public async Task Handle_WhenPassInvalidRole_ReturnsInvalidRolesError()
    {
        // Arrange
        var userRequest = new CreateUserRequest("First", "Last", "Email@test.com", "Pass", ["ExistRole", "InvalidRole"]);

        A.CallTo(() => _userRepo.EmailExistsAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _roleRepo.GetAllNamesAsync(A<bool>.Ignored, A<bool>.Ignored, A<CancellationToken>.Ignored))
            .Returns(["ExistRole"]);

        var command = new AddUserCommand(userRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.InvalidRoles, result.Error);
    }

    [Fact]
    public async Task Handle_ValidUserData_ReturnsSuccess()
    {
        // Arrange
        var userRequest = new CreateUserRequest("First", "Last", "Email@test.com", "Pass", ["Role"]);

        A.CallTo(() => _userRepo.EmailExistsAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _roleRepo.GetAllNamesAsync(A<bool>.Ignored, A<bool>.Ignored, A<CancellationToken>.Ignored))
            .Returns(["Role"]);

        A.CallTo(() => _userRepo.CreateAsync(A<User>.Ignored, A<string>.Ignored, A<bool>.Ignored))
            .Returns(Result.Success());

        A.CallTo(() => _userRepo.AddToRolesAsync(A<User>.Ignored, A<IEnumerable<string>>.Ignored))
            .Returns(Result.Success());

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(Task.CompletedTask);

        var command = new AddUserCommand(userRequest);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(userRequest.Email, result.Value.Email);
        Assert.Equal(userRequest.Roles, result.Value.Roles);

        A.CallTo(() => _userRepo.CreateAsync(A<User>.Ignored, A<string>.Ignored, A<bool>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _userRepo.AddToRolesAsync(A<User>.Ignored, A<IEnumerable<string>>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _cacheService.RemoveByTagAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
