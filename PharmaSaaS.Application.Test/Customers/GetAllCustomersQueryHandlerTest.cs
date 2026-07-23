using PharmaSaaS.Application.Contracts.Customers;
using PharmaSaaS.Application.Features.Customers.GetAll;

namespace PharmaSaaS.Application.Test.Customers;

public class GetAllCustomersQueryHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Customer> _customerRepo = A.Fake<IGenericRepositoryWithPagination<Customer>>();
    private readonly GetAllCustomersQueryHandler _handler;

    public GetAllCustomersQueryHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Customers).Returns(_customerRepo);
        _handler = new GetAllCustomersQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_ReturnsPaginatedList()
    {
        var paginatedList = EmptyPaginatedList.Create<CustomerResponse>();

        A.CallTo(() => _customerRepo.GetPaginatedListAsync<CustomerResponse>(
            A<int>.Ignored, A<int>.Ignored, A<string>.Ignored, A<string>.Ignored,
            A<string>.Ignored, A<string>.Ignored, A<ColumnType>.Ignored, A<string[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(paginatedList);

        var result = await _handler.Handle(new GetAllCustomersQuery(new RequestFilters()), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }
}
