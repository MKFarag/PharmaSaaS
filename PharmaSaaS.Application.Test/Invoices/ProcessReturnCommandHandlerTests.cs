using FakeItEasy;
using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Application.Features.Invoices.ProcessReturn;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Application.Interfaces.Services;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;
using System.Linq.Expressions;
using Xunit;

namespace PharmaSaaS.Application.Test.Invoices;

public class ProcessReturnCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICustomerLoyaltyService _customerLoyaltyService;
    private readonly ProcessReturnCommandHandler _sut;

    public ProcessReturnCommandHandlerTests()
    {
        _unitOfWork = A.Fake<IUnitOfWork>();
        _customerLoyaltyService = A.Fake<ICustomerLoyaltyService>();
        _sut = new ProcessReturnCommandHandler(_unitOfWork, _customerLoyaltyService);
    }

    [Fact]
    public async Task Handle_WhenInvoiceNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var request = new ProcessReturnRequest(new List<ReturnItemRequest> { new ReturnItemRequest(1, 1) });
        var command = new ProcessReturnCommand(1, request, "user1");

        A.CallTo(() => _unitOfWork.Invoices.TrackedFindAsync(A<Expression<Func<Invoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(Task.FromResult<Invoice?>(null));

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenProductNotInInvoice_ReturnsProductNotInInvoiceError()
    {
        // Arrange
        var request = new ProcessReturnRequest(new List<ReturnItemRequest> { new ReturnItemRequest(99, 1) });
        var command = new ProcessReturnCommand(1, request, "user1");

        var invoice = Invoice.Create(1, null, 100m, 0m, 1);
        invoice.Items.Add(InvoiceItem.Create(1, 1, 5, 20m)); // Invoice has product 1, but we request return for product 99

        A.CallTo(() => _unitOfWork.Invoices.TrackedFindAsync(A<Expression<Func<Invoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(invoice);

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.ProductNotInInvoice, result.Error);
    }

    [Fact]
    public async Task Handle_WhenReturnQuantityExceeded_ReturnsReturnQuantityExceededError()
    {
        // Arrange
        var request = new ProcessReturnRequest(new List<ReturnItemRequest> { new ReturnItemRequest(1, 6) }); // returning 6, but bought 5
        var command = new ProcessReturnCommand(1, request, "user1");

        var invoice = Invoice.Create(1, null, 100m, 0m, 1);
        invoice.Items.Add(InvoiceItem.Create(1, 1, 5, 20m)); // 5 bought

        A.CallTo(() => _unitOfWork.Invoices.TrackedFindAsync(A<Expression<Func<Invoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(invoice);

        A.CallTo(() => _unitOfWork.Returns.FindAllAsync(A<Expression<Func<Return, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(new List<Return>());

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.ReturnQuantityExceeded, result.Error);
    }

    [Fact]
    public async Task Handle_WhenShiftNotOpen_ReturnsShiftNotOpenError()
    {
        // Arrange
        var request = new ProcessReturnRequest(new List<ReturnItemRequest> { new ReturnItemRequest(1, 2) });
        var command = new ProcessReturnCommand(1, request, "user1");

        var invoice = Invoice.Create(1, null, 100m, 0m, 1);
        invoice.Items.Add(InvoiceItem.Create(1, 1, 5, 20m));

        A.CallTo(() => _unitOfWork.Invoices.TrackedFindAsync(A<Expression<Func<Invoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(invoice);

        A.CallTo(() => _unitOfWork.Returns.FindAllAsync(A<Expression<Func<Return, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(new List<Return>());

        // Mock shift not found
        A.CallTo(() => _unitOfWork.Shifts.TrackedFindAsync(A<Expression<Func<Shift, bool>>>._, A<CancellationToken>._))
            .Returns(Task.FromResult<Shift?>(null));

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.ShiftNotOpen, result.Error);
    }

    [Fact]
    public async Task Handle_WhenValid_ProcessesReturnAndUpdatesInventoryAndLoyalty()
    {
        // Arrange
        var customerId = 5;
        var request = new ProcessReturnRequest(new List<ReturnItemRequest> { new ReturnItemRequest(1, 2) }); // returning 2
        var command = new ProcessReturnCommand(1, request, "user1");

        // original invoice 100 total, 10 discount -> final 90 (discount factor 90/100 = 0.9)
        var invoice = Invoice.Create(1, customerId, 100m, 10m, 1);
        invoice.Items.Add(InvoiceItem.Create(1, 1, 5, 20m)); 

        var shift = Shift.Create("user1", 100m);
        shift.Id = 2; // return shift

        var customer = Customer.Create("Jane Doe", "0123");
        customer.Id = customerId;

        A.CallTo(() => _unitOfWork.Invoices.TrackedFindAsync(A<Expression<Func<Invoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(invoice);

        A.CallTo(() => _unitOfWork.Returns.FindAllAsync(A<Expression<Func<Return, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(new List<Return>());

        A.CallTo(() => _unitOfWork.Shifts.TrackedFindAsync(A<Expression<Func<Shift, bool>>>._, A<CancellationToken>._))
            .Returns(shift);

        var inventory = Inventory.Create(1, 10, 20m);
        
        A.CallTo(() => _unitOfWork.Inventories.TrackedFindAllAsync(A<Expression<Func<Inventory, bool>>>._, A<CancellationToken>._))
            .Returns(new List<Inventory> { inventory });

        A.CallTo(() => _unitOfWork.Customers.GetAsync(A<object[]>._, A<CancellationToken>._))
            .Returns(customer);

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        Assert.True(result.IsSuccess);
        
        // base refund = 2 * 20 = 40. discount factor = 0.9. total refund = 36.
        Assert.Equal(36m, result.Value.TotalRefund);

        // Verify inventory restored
        Assert.Equal(12, inventory.Quantity); // 10 + 2 = 12

        // Verify shift return added
        Assert.Equal(36m, shift.TotalReturns);

        // Verify loyalty updated
        A.CallTo(() => _customerLoyaltyService.UpdateCustomerLoyaltyAsync(customer, -36m, A<CancellationToken>._)).MustHaveHappenedOnceExactly();

        // Verify transaction commits
        A.CallTo(() => _unitOfWork.BeginTransactionAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.CommitTransactionAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
