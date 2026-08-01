using PharmaSaaS.Application.Features.Expenses.GetAllByShift;
using Xunit;

namespace PharmaSaaS.Application.Test.Expenses;

public class GetAllExpensesByShiftQueryHandlerTests : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Expense> _expenseRepo = A.Fake<IGenericRepositoryWithPagination<Expense>>();
    private readonly GetAllExpensesByShiftQueryHandler _handler;

    public GetAllExpensesByShiftQueryHandlerTests(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Expenses).Returns(_expenseRepo);
        _handler = new GetAllExpensesByShiftQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenValid_ReturnsExpenses()
    {
        // Arrange
        var expenses = new List<Expense>
        {
            Expense.Create(1, 100m, "Internet"),
            Expense.Create(1, 50m, "Cleaning")
        };

        A.CallTo(() => _expenseRepo.FindAllAsync(A<Expression<Func<Expense, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(expenses);

        var query = new GetAllExpensesByShiftQuery(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(100m, result.Value[0].Amount);
        Assert.Equal(50m, result.Value[1].Amount);
    }
}
