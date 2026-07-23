using PharmaSaaS.Application.Contracts.Customers;
using PharmaSaaS.Application.Features.Customers.Update;

namespace PharmaSaaS.Application.Test.Customers;

public class UpdateCustomerCommandHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Customer> _customerRepo = A.Fake<IGenericRepositoryWithPagination<Customer>>();
    private readonly UpdateCustomerCommandHandler _handler;

    public UpdateCustomerCommandHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Customers).Returns(_customerRepo);
        _handler = new UpdateCustomerCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ReturnsNotFound()
    {
        A.CallTo(() => _customerRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns((Customer?)null);

        var result = await _handler.Handle(new UpdateCustomerCommand(1, new CustomerRequest("A", "B")), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        var customer = Customer.Create("Ahmed", "010");
        A.CallTo(() => _customerRepo.GetAsync(A<object[]>.Ignored, A<CancellationToken>.Ignored))
            .Returns(customer);
        A.CallTo(() => _customerRepo.AnyAsync(A<Expression<Func<Customer, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        var result = await _handler.Handle(new UpdateCustomerCommand(1, new CustomerRequest("Ali", "011")), CancellationToken.None);

        Assert.True(result.IsSuccess);
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
    }
}
