using PharmaSaaS.Application.Features.Users.Get;

namespace PharmaSaaS.Application.Test.Users;

public class GetUserQueryHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IUserRepository _userRepo = A.Fake<IUserRepository>();
    private readonly GetUserQueryHandler _handler;

    public GetUserQueryHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Users).Returns(_userRepo);
        _handler = new GetUserQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var user = new User { Id = Guid.CreateVersion7().ToString() };

        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns((User?)null);

        var query = new GetUserQuery(user.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_ValidId_ReturnsUser()
    {
        // Arrange
        var user = new User { Id = Guid.CreateVersion7().ToString() };

        A.CallTo(() => _userRepo.FindByIdAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
            .Returns(user);

        A.CallTo(() => _userRepo.GetRolesAsync(A<User>.Ignored))
            .Returns([]);

        var query = new GetUserQuery(user.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value.Id);
    }
}
