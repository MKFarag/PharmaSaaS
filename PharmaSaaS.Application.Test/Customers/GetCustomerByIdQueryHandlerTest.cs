using PharmaSaaS.Application.Features.Customers.Get;

namespace PharmaSaaS.Application.Test.Customers;

public class GetCustomerByIdQueryHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Customer> _customerRepo = A.Fake<IGenericRepositoryWithPagination<Customer>>();
    private readonly GetCustomerByIdQueryHandler _handler;

    public GetCustomerByIdQueryHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Customers).Returns(_customerRepo);
        _handler = new GetCustomerByIdQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsNotFound()
    {
        A.CallTo(() => _customerRepo.FindAsync(A<Expression<Func<Customer, bool>>>.Ignored, A<string[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Customer?)null);

        var result = await _handler.Handle(new GetCustomerByIdQuery(1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenFound_ReturnsSuccess()
    {
        var customer = Customer.Create("Ahmed", "010");
        A.CallTo(() => _customerRepo.FindAsync(A<Expression<Func<Customer, bool>>>.Ignored, A<string[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(customer);

        var result = await _handler.Handle(new GetCustomerByIdQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Ahmed", result.Value.FullName);
    }
}
