using System.Linq.Expressions;
using PharmaSaaS.Application.Contracts.Shifts;
using PharmaSaaS.Application.Features.Shifts.GetAll;

namespace PharmaSaaS.Application.Test.Shifts;

public class GetAllShiftsQueryHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Shift> _shiftRepo = A.Fake<IGenericRepositoryWithPagination<Shift>>();
    private readonly GetAllShiftsQueryHandler _handler;

    public GetAllShiftsQueryHandlerTests(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Shifts).Returns(_shiftRepo);
        _handler = new GetAllShiftsQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenDateProvided_ReturnsFilteredPaginatedList()
    {
        // Arrange
        var date = new DateOnly(2026, 7, 23);
        var request = new GetAllShiftsQuery(new ShiftRequestFilters { PageNumber = 1, PageSize = 10, Date = date });
        var expectedList = A.Fake<IPaginatedList<ShiftResponse>>();

        A.CallTo(() => _shiftRepo.FindPaginatedListAsync<ShiftResponse>(
            A<Expression<Func<Shift, bool>>>.Ignored,
            1, 10, nameof(Shift.Id), A<CancellationToken>.Ignored))
            .Returns(expectedList);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Same(expectedList, result);
        A.CallTo(() => _shiftRepo.FindPaginatedListAsync<ShiftResponse>(
            A<Expression<Func<Shift, bool>>>.Ignored,
            1, 10, nameof(Shift.Id), A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenDateNotProvided_ReturnsPaginatedList()
    {
        // Arrange
        var request = new GetAllShiftsQuery(new ShiftRequestFilters { PageNumber = 2, PageSize = 20 });
        var expectedList = A.Fake<IPaginatedList<ShiftResponse>>();

        A.CallTo(() => _shiftRepo.GetPaginatedListAsync<ShiftResponse>(
            2, 20, nameof(Shift.Id), A<CancellationToken>.Ignored))
            .Returns(expectedList);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Same(expectedList, result);
        A.CallTo(() => _shiftRepo.GetPaginatedListAsync<ShiftResponse>(
            2, 20, nameof(Shift.Id), A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
