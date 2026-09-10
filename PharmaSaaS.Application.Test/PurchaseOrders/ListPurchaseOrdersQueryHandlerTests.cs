using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Application.Features.PurchaseOrders.List;

namespace PharmaSaaS.Application.Test.PurchaseOrders;

public class ListPurchaseOrdersQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();

    [Fact]
    public async Task Handle_WhenCalled_DelegatesFiltersToPaginatedRepo()
    {
        // Arrange
        var filters = new PurchaseOrderRequestFilters { PageNumber = 1, PageSize = 10 };
        var query = new ListPurchaseOrdersQuery(filters);

        var paginatedResult = A.Fake<IPaginatedList<PurchaseOrderResponse>>();
        A.CallTo(() => _unitOfWork.PurchaseOrders.FindPaginatedListAsync<PurchaseOrderResponse>(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                A<int>._,
                A<int>._,
                A<string>._,
                A<string[]>._,
                A<CancellationToken>._))
            .Returns(paginatedResult);

        var handler = new ListPurchaseOrdersQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.Same(paginatedResult, result);
        A.CallTo(() => _unitOfWork.PurchaseOrders.FindPaginatedListAsync<PurchaseOrderResponse>(
                A<Expression<Func<PurchaseOrder, bool>>>._,
                filters.PageNumber,
                filters.PageSize,
                A<string>._,
                A<string[]>._,
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
