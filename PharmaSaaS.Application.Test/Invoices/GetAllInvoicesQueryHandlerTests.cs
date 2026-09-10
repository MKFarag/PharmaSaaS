using PharmaSaaS.Application.Features.Invoices.GetAll;

namespace PharmaSaaS.Application.Test.Invoices;

public class GetAllInvoicesQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly GetAllInvoicesQueryHandler _sut;

    public GetAllInvoicesQueryHandlerTests()
    {
        _unitOfWork = A.Fake<IUnitOfWork>();
        _sut = new GetAllInvoicesQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_Always_ReturnsPaginatedList()
    {
        // Arrange
        var filters = new InvoiceRequestFilters { PageNumber = 1, PageSize = 10 };
        var query = new GetAllInvoicesQuery(filters);

        var paginatedList = A.Fake<IPaginatedList<InvoiceResponse>>();

        A.CallTo(() => _unitOfWork.Invoices.FindPaginatedListAsync<InvoiceResponse>(
            A<Expression<Func<Invoice, bool>>>._,
            1,
            10,
            nameof(Invoice.Id),
            A<string[]>._,
            A<CancellationToken>._)).Returns(paginatedList);

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        Assert.Equal(paginatedList, result);
    }
}
