using FakeItEasy;
using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Application.Features.Invoices.Create;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Application.Interfaces.Services;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;
using System.Linq.Expressions;
using Xunit;

namespace PharmaSaaS.Application.Test.Invoices;

public class CreateInvoiceCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICustomerLoyaltyService _customerLoyaltyService;
    private readonly CreateInvoiceCommandHandler _sut;

    public CreateInvoiceCommandHandlerTests()
    {
        _unitOfWork = A.Fake<IUnitOfWork>();
        _customerLoyaltyService = A.Fake<ICustomerLoyaltyService>();
        _sut = new CreateInvoiceCommandHandler(_unitOfWork, _customerLoyaltyService);
    }

    [Fact]
    public async Task Handle_WhenShiftNotOpen_ReturnsShiftNotOpenError()
    {
        // Arrange
        var command = new CreateInvoiceCommand(new CreateInvoiceRequest(null, new List<InvoiceItemRequest>()), "user1");

        A.CallTo(() => _unitOfWork.Shifts.TrackedFindAsync(A<Expression<Func<Shift, bool>>>._, A<CancellationToken>._))
            .Returns(Task.FromResult<Shift?>(null));

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.ShiftNotOpen, result.Error);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsProductNotFoundError()
    {
        // Arrange
        var shift = Shift.Create("user1", 100m);
        var request = new CreateInvoiceRequest(null, new List<InvoiceItemRequest> { new InvoiceItemRequest(1, 2) });
        var command = new CreateInvoiceCommand(request, "user1");

        A.CallTo(() => _unitOfWork.Shifts.TrackedFindAsync(A<Expression<Func<Shift, bool>>>._, A<CancellationToken>._))
            .Returns(shift);

        // Return empty list of inventories to simulate not found
        A.CallTo(() => _unitOfWork.Inventories.TrackedFindAllAsync(A<Expression<Func<Inventory, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(new List<Inventory>());

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.ProductNotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenInsufficientStock_ReturnsInsufficientStockError()
    {
        // Arrange
        var shift = Shift.Create("user1", 100m);
        var request = new CreateInvoiceRequest(null, new List<InvoiceItemRequest> { new InvoiceItemRequest(1, 10) }); // Requesting 10
        var command = new CreateInvoiceCommand(request, "user1");

        A.CallTo(() => _unitOfWork.Shifts.TrackedFindAsync(A<Expression<Func<Shift, bool>>>._, A<CancellationToken>._))
            .Returns(shift);

        var inventory = Inventory.Create(1, 5, 100m); // Only 5 in stock
        // selling price is set in Create

        A.CallTo(() => _unitOfWork.Inventories.TrackedFindAllAsync(A<Expression<Func<Inventory, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(new List<Inventory> { inventory });

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.InsufficientStock, result.Error);
    }

    [Fact]
    public async Task Handle_WhenValidAndNoCustomer_CreatesInvoiceAndCompletesTransaction()
    {
        // Arrange
        var shift = Shift.Create("user1", 100m);
        shift.Id = 1;

        var request = new CreateInvoiceRequest(null, new List<InvoiceItemRequest> { new InvoiceItemRequest(1, 2) });
        var command = new CreateInvoiceCommand(request, "user1");

        A.CallTo(() => _unitOfWork.Shifts.TrackedFindAsync(A<Expression<Func<Shift, bool>>>._, A<CancellationToken>._))
            .Returns(shift);

        var inventory = Inventory.Create(1, 5, 100m); // 5 in stock, 100m price
        var inventories = new List<Inventory> { inventory };

        A.CallTo(() => _unitOfWork.Inventories.TrackedFindAllAsync(A<Expression<Func<Inventory, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(inventories);

        var fakeInvoice = Invoice.Create(1, null, 200m, 0m, 1);
        fakeInvoice.Id = 99;

        // Mock FindAsync to return the mapped response later
        A.CallTo(() => _unitOfWork.Invoices.FindAsync(A<Expression<Func<Invoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(fakeInvoice);

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(99, result.Value.Id);

        // Verify transaction handling
        A.CallTo(() => _unitOfWork.BeginTransactionAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.CommitTransactionAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.RollbackTransactionAsync()).MustNotHaveHappened();

        // Verify deduct stock
        Assert.Equal(3, inventory.Quantity); // 5 - 2 = 3

        // Verify shift sale added
        Assert.Equal(200m, shift.TotalSales);
    }

    [Fact]
    public async Task Handle_WhenValidWithCustomer_CalculatesDiscountAndUpdatesLoyalty()
    {
        // Arrange
        var shift = Shift.Create("user1", 100m);
        shift.Id = 1;

        var customerId = 5;
        var request = new CreateInvoiceRequest(customerId, new List<InvoiceItemRequest> { new InvoiceItemRequest(1, 2) });
        var command = new CreateInvoiceCommand(request, "user1");

        var customer = Customer.Create("Jane Doe", "0123");
        customer.Id = customerId;

        A.CallTo(() => _unitOfWork.Shifts.TrackedFindAsync(A<Expression<Func<Shift, bool>>>._, A<CancellationToken>._))
            .Returns(shift);

        var inventory = Inventory.Create(1, 5, 100m);
        
        A.CallTo(() => _unitOfWork.Inventories.TrackedFindAllAsync(A<Expression<Func<Inventory, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(new List<Inventory> { inventory });

        A.CallTo(() => _unitOfWork.Customers.GetAsync(A<object[]>._, A<CancellationToken>._))
            .Returns(customer);

        A.CallTo(() => _customerLoyaltyService.CalculateDiscountAsync(customer, 200m, A<CancellationToken>._))
            .Returns(50m); // 50 discount -> final amount 150

        var fakeInvoice = Invoice.Create(1, customerId, 200m, 50m, 1);
        fakeInvoice.Id = 99;

        A.CallTo(() => _unitOfWork.Invoices.FindAsync(A<Expression<Func<Invoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(fakeInvoice);

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(150m, fakeInvoice.FinalAmount);

        // Loyalty updates check
        A.CallTo(() => _customerLoyaltyService.UpdateCustomerLoyaltyAsync(customer, 150m, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        
        // Transaction check
        A.CallTo(() => _unitOfWork.CommitTransactionAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
