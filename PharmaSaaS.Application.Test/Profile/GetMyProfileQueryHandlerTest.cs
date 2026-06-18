using PharmaSaaS.Application.Features.Profile.Get;

namespace PharmaSaaS.Application.Test.Profile;

public class GetMyProfileQueryHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IUserRepository _userRepo = A.Fake<IUserRepository>();
    private readonly GetMyProfileQueryHandler _handler;

    public GetMyProfileQueryHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Users).Returns(_userRepo);
        _handler = new GetMyProfileQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns((User?)null);

        var query = new GetMyProfileQuery(Guid.CreateVersion7().ToString());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_ValidId_ReturnsProfile()
    {
        // Arrange
        var user = User.Create("user@example.com", "John", "Doe");

        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(user);

        var query = new GetMyProfileQuery(Guid.CreateVersion7().ToString());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(user.FirstName, result.Value.FirstName);
        Assert.Equal(user.LastName, result.Value.LastName);
        Assert.Equal(user.Email, result.Value.Email);
    }
}
