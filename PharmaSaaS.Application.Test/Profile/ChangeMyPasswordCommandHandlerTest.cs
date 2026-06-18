using PharmaSaaS.Application.Features.Profile.ChangePassword;

namespace PharmaSaaS.Application.Test.Profile;

public class ChangeMyPasswordCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IUserRepository _userRepo = A.Fake<IUserRepository>();
    private readonly ChangeMyPasswordCommandHandler _handler;

    public ChangeMyPasswordCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Users).Returns(_userRepo);
        _handler = new ChangeMyPasswordCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns((User?)null);

        var command = new ChangeMyPasswordCommand(Guid.CreateVersion7().ToString(), "OldPass123!", "NewPass456!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenCurrentPasswordIsIncorrect_ReturnsFailure()
    {
        // Arrange
        var user = User.Create("user@example.com", "John", "Doe");

        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(user);

        A.CallTo(() => _userRepo.ChangePasswordAsync(A<User>.Ignored, A<string>.Ignored, A<string>.Ignored))
            .Returns(Result.Failure(UserErrors.InvalidCredentials));

        var command = new ChangeMyPasswordCommand(Guid.CreateVersion7().ToString(), "WrongPass!", "NewPass456!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.InvalidCredentials, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        // Arrange
        var user = User.Create("user@example.com", "John", "Doe");

        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(user);

        A.CallTo(() => _userRepo.ChangePasswordAsync(A<User>.Ignored, A<string>.Ignored, A<string>.Ignored))
            .Returns(Result.Success());

        var command = new ChangeMyPasswordCommand(Guid.CreateVersion7().ToString(), "OldPass123!", "NewPass456!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        A.CallTo(() => _userRepo.ChangePasswordAsync(A<User>.Ignored, A<string>.Ignored, A<string>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
