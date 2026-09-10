using PharmaSaaS.Application.Contracts.Shifts;
using PharmaSaaS.Application.Features.Shifts.Open;

namespace PharmaSaaS.Application.Test.Shifts;

public class OpenShiftCommandHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Shift> _shiftRepo = A.Fake<IGenericRepositoryWithPagination<Shift>>();
    private readonly OpenShiftCommandHandler _handler;

    public OpenShiftCommandHandlerTests(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Shifts).Returns(_shiftRepo);
        _handler = new OpenShiftCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenOpenShiftAlreadyExistsForToday_ReturnsConflictError()
    {
        // Arrange
        A.CallTo(() => _shiftRepo.AnyAsync(A<Expression<Func<Shift, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var request = new OpenShiftRequest(100);
        var command = new OpenShiftCommand(request, "user1");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ShiftErrors.AlreadyOpen, result.Error);

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenNoOpenShiftExists_CreatesShiftAndReturnsResponse()
    {
        // Arrange
        A.CallTo(() => _shiftRepo.AnyAsync(A<Expression<Func<Shift, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        var request = new OpenShiftRequest(100);
        var command = new OpenShiftCommand(request, "user1");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.CurrentBalance);
        Assert.Equal(ShiftStatus.Open, result.Value.Status);

        A.CallTo(() => _shiftRepo.AddAsync(A<Shift>.That.Matches(s => s.OpenedById == "user1" && s.OpeningBalance == 100), A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
