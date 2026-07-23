using PharmaSaaS.Application.Contracts.Shifts;
using PharmaSaaS.Application.Features.Shifts.GetById;

namespace PharmaSaaS.Application.Test.Shifts;

public class GetShiftByIdQueryHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Shift> _shiftRepo = A.Fake<IGenericRepositoryWithPagination<Shift>>();
    private readonly GetShiftByIdQueryHandler _handler;

    public GetShiftByIdQueryHandlerTests(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Shifts).Returns(_shiftRepo);
        _handler = new GetShiftByIdQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenShiftNotFound_ReturnsNotFoundError()
    {
        // Arrange
        A.CallTo(() => _shiftRepo.GetAsync(A<object[]>.That.IsSameSequenceAs(new object[] { 1 }), A<CancellationToken>.Ignored))
            .Returns((Shift?)null);

        // Act
        var result = await _handler.Handle(new GetShiftByIdQuery(1), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ShiftErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenShiftExists_ReturnsShiftDetailResponse()
    {
        // Arrange
        var shift = Shift.Create("user1", 100);
        shift.Id = 1;

        A.CallTo(() => _shiftRepo.GetAsync(A<object[]>.That.IsSameSequenceAs(new object[] { 1 }), A<CancellationToken>.Ignored))
            .Returns(shift);

        // Act
        var result = await _handler.Handle(new GetShiftByIdQuery(1), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Id);
        Assert.Equal("user1", result.Value.OpenedById);
        Assert.Equal(100, result.Value.OpeningBalance);
    }
}
