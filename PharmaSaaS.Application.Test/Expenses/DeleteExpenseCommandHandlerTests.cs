using PharmaSaaS.Application.Features.Expenses.Delete;
using Xunit;

namespace PharmaSaaS.Application.Test.Expenses;

public class DeleteExpenseCommandHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Shift> _shiftRepo = A.Fake<IGenericRepositoryWithPagination<Shift>>();
    private readonly IGenericRepositoryWithPagination<Expense> _expenseRepo = A.Fake<IGenericRepositoryWithPagination<Expense>>();
    private readonly DeleteExpenseCommandHandler _handler;

    public DeleteExpenseCommandHandlerTests(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Shifts).Returns(_shiftRepo);
        A.CallTo(() => _unitOfWork.Expenses).Returns(_expenseRepo);
        _handler = new DeleteExpenseCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenExpenseNotFound_ReturnsNotFoundError()
    {
        // Arrange
        A.CallTo(() => _expenseRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Expense?)null);

        var command = new DeleteExpenseCommand(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ExpenseErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenShiftNotOpen_ReturnsNotOpenError()
    {
        // Arrange
        var expense = Expense.Create(1, 100m, "Internet Bill");
        var shift = Shift.Create("user1", 1000m);
        shift.Close(1000m); // Close the shift

        A.CallTo(() => _expenseRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(expense);
        A.CallTo(() => _shiftRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(shift);

        var command = new DeleteExpenseCommand(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ShiftErrors.NotOpen, result.Error);
    }

    [Fact]
    public async Task Handle_WhenValid_DeletesExpenseAndUpdatesShift()
    {
        // Arrange
        var shift = Shift.Create("user1", 1000m);
        shift.AddExpense(100m);
        
        var expense = Expense.Create(1, 100m, "Internet Bill");

        A.CallTo(() => _expenseRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(expense);
        A.CallTo(() => _shiftRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(shift);

        var command = new DeleteExpenseCommand(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0m, shift.TotalExpenses); // Shift was updated

        A.CallTo(() => _expenseRepo.Delete(expense))
            .MustHaveHappenedOnceExactly();
        
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
