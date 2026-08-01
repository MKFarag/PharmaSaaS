using PharmaSaaS.Application.Contracts.Expenses;
using PharmaSaaS.Application.Features.Expenses.Add;
using Xunit;

namespace PharmaSaaS.Application.Test.Expenses;

public class LogExpenseCommandHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Shift> _shiftRepo = A.Fake<IGenericRepositoryWithPagination<Shift>>();
    private readonly IGenericRepositoryWithPagination<Expense> _expenseRepo = A.Fake<IGenericRepositoryWithPagination<Expense>>();
    private readonly LogExpenseCommandHandler _handler;

    public LogExpenseCommandHandlerTests(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Shifts).Returns(_shiftRepo);
        A.CallTo(() => _unitOfWork.Expenses).Returns(_expenseRepo);
        _handler = new LogExpenseCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenShiftNotOpen_ReturnsNotOpenError()
    {
        // Arrange
        A.CallTo(() => _shiftRepo.TrackedFindAsync(A<Expression<Func<Shift, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Shift?)null);

        var request = new ExpenseRequest(100, "Internet Bill");
        var command = new LogExpenseCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ShiftErrors.NotOpen, result.Error);
    }

    [Fact]
    public async Task Handle_WhenValid_LogsExpenseAndUpdatesShift()
    {
        // Arrange
        var shift = Shift.Create("user1", 1000m); // Initially has 0 TotalExpenses
        A.CallTo(() => _shiftRepo.TrackedFindAsync(A<Expression<Func<Shift, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(shift);

        var request = new ExpenseRequest(100m, "Internet Bill");
        var command = new LogExpenseCommand(request);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(100m, shift.TotalExpenses); // Shift was updated

        A.CallTo(() => _expenseRepo.AddAsync(A<Expense>.That.Matches(e => e.Amount == 100m && e.Description == "Internet Bill"), A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
        
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
