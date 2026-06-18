using PharmaSaaS.Application.Features.Profile.ConfirmChangeEmail;

namespace PharmaSaaS.Application.Test.Profile;

public class ConfirmChangeMyEmailCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IUserRepository _userRepo = A.Fake<IUserRepository>();
    private readonly IUrlEncoder _urlEncoder = A.Fake<IUrlEncoder>();
    private readonly INotificationService _notificationService = A.Fake<INotificationService>();
    private readonly ConfirmChangeMyEmailCommandHandler _handler;

    public ConfirmChangeMyEmailCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Users).Returns(_userRepo);
        _handler = new ConfirmChangeMyEmailCommandHandler(_unitOfWork, _urlEncoder, _notificationService);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns((User?)null);

        var command = new ConfirmChangeMyEmailCommand(Guid.CreateVersion7().ToString(), "new@example.com", "some-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenTokenIsInvalid_ReturnsInvalidTokenError()
    {
        // Arrange
        var user = User.Create("current@example.com", "John", "Doe");

        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(user);

        A.CallTo(() => _urlEncoder.Decode(A<string>.Ignored))
            .Returns(null);

        var command = new ConfirmChangeMyEmailCommand(Guid.CreateVersion7().ToString(), "new@example.com", "invalid-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.InvalidToken, result.Error);
    }

    [Fact]
    public async Task Handle_WhenChangeEmailFails_ReturnsFailure()
    {
        // Arrange
        var user = User.Create("current@example.com", "John", "Doe");
        const string decodedToken = "decoded-token";

        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(user);

        A.CallTo(() => _urlEncoder.Decode(A<string>.Ignored))
            .Returns(decodedToken);

        A.CallTo(() => _userRepo.ChangeEmailAsync(A<User>.Ignored, A<string>.Ignored, A<string>.Ignored))
            .Returns(Result.Failure(UserErrors.InvalidToken));

        var command = new ConfirmChangeMyEmailCommand(Guid.CreateVersion7().ToString(), "new@example.com", "encoded-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.InvalidToken, result.Error);

        A.CallTo(() => _notificationService.SendChangeEmailNotificationAsync(A<User>.Ignored, A<string>.Ignored, A<DateTime>.Ignored))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_ValidToken_ChangesEmailAndSendsNotification()
    {
        // Arrange
        var user = User.Create("current@example.com", "John", "Doe");
        const string decodedToken = "decoded-token";

        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(user);

        A.CallTo(() => _urlEncoder.Decode(A<string>.Ignored))
            .Returns(decodedToken);

        A.CallTo(() => _userRepo.ChangeEmailAsync(A<User>.Ignored, A<string>.Ignored, decodedToken))
            .Returns(Result.Success());

        A.CallTo(() => _notificationService.SendChangeEmailNotificationAsync(A<User>.Ignored, A<string>.Ignored, A<DateTime>.Ignored))
            .Returns(Task.CompletedTask);

        var command = new ConfirmChangeMyEmailCommand(Guid.CreateVersion7().ToString(), "new@example.com", "encoded-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        A.CallTo(() => _userRepo.ChangeEmailAsync(A<User>.Ignored, A<string>.Ignored, decodedToken))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _notificationService.SendChangeEmailNotificationAsync(A<User>.Ignored, A<string>.Ignored, A<DateTime>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
