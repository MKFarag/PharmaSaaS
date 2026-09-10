using PharmaSaaS.Application.Contracts.Expenses;
using PharmaSaaS.Application.Features.Expenses.GetAllByShift;

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
        var expenseResponses = new List<ExpenseResponse>
        {
            new ExpenseResponse(1, 100m, "Internet", DateTimeOffset.UtcNow, 1),
            new ExpenseResponse(2, 50m, "Cleaning", DateTimeOffset.UtcNow, 1)
        };

        A.CallTo(() => _expenseRepo.FindAllProjectionAsync<ExpenseResponse>(
                A<Expression<Func<Expense, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(expenseResponses);

        var query = new GetAllExpensesByShiftQuery(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var value = result.Value.ToList();
        Assert.Equal(2, value.Count);
        Assert.Equal(100m, value[0].Amount);
        Assert.Equal(50m, value[1].Amount);
    }
}
