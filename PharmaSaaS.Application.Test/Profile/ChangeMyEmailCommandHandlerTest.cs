using PharmaSaaS.Application.Features.Profile.ChangeEmail;

namespace PharmaSaaS.Application.Test.Profile;

public class ChangeMyEmailCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IUserRepository _userRepo = A.Fake<IUserRepository>();
    private readonly IUrlEncoder _urlEncoder = A.Fake<IUrlEncoder>();
    private readonly INotificationService _notificationService = A.Fake<INotificationService>();
    private readonly ChangeMyEmailCommandHandler _handler;

    public ChangeMyEmailCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Users).Returns(_userRepo);
        _handler = new ChangeMyEmailCommandHandler(_unitOfWork, _urlEncoder, _notificationService);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns((User?)null);

        var command = new ChangeMyEmailCommand(Guid.CreateVersion7().ToString(), "new@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenNewEmailSameAsCurrent_ReturnsSameEmailError()
    {
        // Arrange
        var user = User.Create("same@example.com", "John", "Doe");

        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(user);

        var command = new ChangeMyEmailCommand(Guid.CreateVersion7().ToString(), "same@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.SameEmail, result.Error);
    }

    [Fact]
    public async Task Handle_WhenNewEmailAlreadyExists_ReturnsDuplicatedEmailError()
    {
        // Arrange
        var user = User.Create("current@example.com", "John", "Doe");

        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(user);

        A.CallTo(() => _userRepo.EmailExistsAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var command = new ChangeMyEmailCommand(Guid.CreateVersion7().ToString(), "taken@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.DuplicatedEmail, result.Error);
    }

    [Fact]
    public async Task Handle_ValidNewEmail_SendsConfirmationAndReturnsSuccess()
    {
        // Arrange
        var user = User.Create("current@example.com", "John", "Doe");
        const string rawToken = "raw-token";
        const string encodedToken = "encoded-token";

        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(user);

        A.CallTo(() => _userRepo.EmailExistsAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        A.CallTo(() => _userRepo.GenerateChangeEmailTokenAsync(A<User>.Ignored, A<string>.Ignored))
            .Returns(rawToken);

        A.CallTo(() => _urlEncoder.Encode(rawToken))
            .Returns(encodedToken);

        A.CallTo(() => _notificationService.SendConfirmationLinkAsync(A<User>.Ignored, A<string>.Ignored, A<int>.Ignored))
            .Returns(Task.CompletedTask);

        var command = new ChangeMyEmailCommand(Guid.CreateVersion7().ToString(), "new@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        A.CallTo(() => _userRepo.GenerateChangeEmailTokenAsync(A<User>.Ignored, A<string>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _urlEncoder.Encode(rawToken))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _notificationService.SendConfirmationLinkAsync(A<User>.Ignored, encodedToken, A<int>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
