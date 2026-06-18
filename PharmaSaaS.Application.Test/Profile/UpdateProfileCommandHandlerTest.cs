using PharmaSaaS.Application.Contracts.Profile;
using PharmaSaaS.Application.Features.Profile.Update;

namespace PharmaSaaS.Application.Test.Profile;

public class UpdateProfileCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IUserRepository _userRepo = A.Fake<IUserRepository>();
    private readonly UpdateProfileCommandHandler _handler;

    public UpdateProfileCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Users).Returns(_userRepo);
        _handler = new UpdateProfileCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns((User?)null);

        var command = new UpdateProfileCommand(Guid.CreateVersion7().ToString(), new UpdateUserProfileRequest("John", "Doe"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        // Arrange
        var user = User.Create("user@example.com", "OldFirst", "OldLast");
        var request = new UpdateUserProfileRequest("John", "Doe");

        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(user);

        A.CallTo(() => _userRepo.UpdateAsync(A<User>.Ignored))
            .Returns(Result.Success());

        var command = new UpdateProfileCommand(Guid.CreateVersion7().ToString(), request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        A.CallTo(() => _userRepo.UpdateAsync(A<User>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
