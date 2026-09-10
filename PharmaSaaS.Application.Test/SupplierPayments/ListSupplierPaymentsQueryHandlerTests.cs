using PharmaSaaS.Application.Contracts.SupplierPayments;
using PharmaSaaS.Application.Features.SupplierPayments.List;

namespace PharmaSaaS.Application.Test.SupplierPayments;

public class ListSupplierPaymentsQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();

    [Fact]
    public async Task Handle_WhenCalled_DelegatesFiltersToPaginatedRepo()
    {
        // Arrange
        var filters = new SupplierPaymentRequestFilters { PageNumber = 1, PageSize = 10 };
        var query = new ListSupplierPaymentsQuery(filters);

        var paginatedResult = A.Fake<IPaginatedList<SupplierPaymentResponse>>();
        A.CallTo(() => _unitOfWork.SupplierPayments.FindPaginatedListAsync<SupplierPaymentResponse>(
                A<Expression<Func<SupplierPayment, bool>>>._,
                A<int>._,
                A<int>._,
                A<string>._,
                A<string[]>._,
                A<CancellationToken>._))
            .Returns(paginatedResult);

        var handler = new ListSupplierPaymentsQueryHandler(_unitOfWork);

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.Same(paginatedResult, result);
        A.CallTo(() => _unitOfWork.SupplierPayments.FindPaginatedListAsync<SupplierPaymentResponse>(
                A<Expression<Func<SupplierPayment, bool>>>._,
                filters.PageNumber,
                filters.PageSize,
                A<string>._,
                A<string[]>._,
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
