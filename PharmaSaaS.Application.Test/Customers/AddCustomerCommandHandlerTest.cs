using PharmaSaaS.Application.Contracts.Customers;
using PharmaSaaS.Application.Features.Customers.Add;

namespace PharmaSaaS.Application.Test.Customers;

public class AddCustomerCommandHandlerTest : IClassFixture<MapsterTestFixture>
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Customer> _customerRepo = A.Fake<IGenericRepositoryWithPagination<Customer>>();
    private readonly AddCustomerCommandHandler _handler;

    public AddCustomerCommandHandlerTest(MapsterTestFixture _)
    {
        A.CallTo(() => _unitOfWork.Customers).Returns(_customerRepo);
        _handler = new AddCustomerCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenPhoneNumberDuplicated_ReturnsError()
    {
        A.CallTo(() => _customerRepo.AnyAsync(A<Expression<Func<Customer, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(true);

        var result = await _handler.Handle(new AddCustomerCommand(new CustomerRequest("Ahmed", "010")), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(CustomerErrors.DuplicatedPhoneNumber, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        A.CallTo(() => _customerRepo.AnyAsync(A<Expression<Func<Customer, bool>>>.Ignored, A<CancellationToken>.Ignored))
            .Returns(false);

        var result = await _handler.Handle(new AddCustomerCommand(new CustomerRequest("Ahmed", "010")), CancellationToken.None);

        Assert.True(result.IsSuccess);
        A.CallTo(() => _customerRepo.AddAsync(A<Customer>.Ignored, A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
