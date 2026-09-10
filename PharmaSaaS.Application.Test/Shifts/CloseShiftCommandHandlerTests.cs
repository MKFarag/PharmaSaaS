using PharmaSaaS.Application.Contracts.Shifts;
using PharmaSaaS.Application.Features.Shifts.Close;

namespace PharmaSaaS.Application.Test.Shifts;

public class CloseShiftCommandHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Shift> _shiftRepo = A.Fake<IGenericRepositoryWithPagination<Shift>>();
    private readonly CloseShiftCommandHandler _handler;

    public CloseShiftCommandHandlerTests(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Shifts).Returns(_shiftRepo);
        _handler = new CloseShiftCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenShiftNotFound_ReturnsNotFoundError()
    {
        // Arrange
        A.CallTo(() => _shiftRepo.GetAsync(A<object[]>.That.IsSameSequenceAs(new object[] { 1 }), A<CancellationToken>.Ignored))
            .Returns((Shift?)null);

        var request = new CloseShiftRequest(500);
        var command = new CloseShiftCommand(1, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ShiftErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenShiftNotOpen_ReturnsNotOpenError()
    {
        // Arrange
        var shift = Shift.Create("user1", 100);
        shift.Id = 1;
        shift.Close(100); // Now it's closed

        A.CallTo(() => _shiftRepo.GetAsync(A<object[]>.That.IsSameSequenceAs(new object[] { 1 }), A<CancellationToken>.Ignored))
            .Returns(shift);

        var request = new CloseShiftRequest(500);
        var command = new CloseShiftCommand(1, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ShiftErrors.NotOpen, result.Error);

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenShiftIsOpen_ClosesShiftAndReturnsResponse()
    {
        // Arrange
        var shift = Shift.Create("user1", 100);
        // ExpectedBalance should be OpeningBalance (100) + Sales (0) - Returns (0) - Expenses (0) = 100
        shift.Id = 1;

        A.CallTo(() => _shiftRepo.GetAsync(A<object[]>.That.IsSameSequenceAs(new object[] { 1 }), A<CancellationToken>.Ignored))
            .Returns(shift);

        var request = new CloseShiftRequest(150); // Found 150 in drawer (50 discrepancy)
        var command = new CloseShiftCommand(1, request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value.ExpectedBalance);
        Assert.Equal(50, result.Value.Discrepancy);

        Assert.Equal(ShiftStatus.Closed, shift.Status);
        Assert.Equal(150, shift.ActualBalance);
        Assert.Equal(50, shift.Discrepancy);

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
