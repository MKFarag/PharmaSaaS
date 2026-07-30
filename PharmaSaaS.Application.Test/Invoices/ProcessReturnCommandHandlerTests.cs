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
    private readonly IReturnItemValidatorService _returnItemValidator;
    private readonly IReturnRefundCalculatorService _refundCalculator;
    private readonly ICustomerLoyaltyService _customerLoyaltyService;
    private readonly ProcessReturnCommandHandler _sut;

    public ProcessReturnCommandHandlerTests()
    {
        _unitOfWork = A.Fake<IUnitOfWork>();
        _returnItemValidator = A.Fake<IReturnItemValidatorService>();
        _refundCalculator = A.Fake<IReturnRefundCalculatorService>();
        _customerLoyaltyService = A.Fake<ICustomerLoyaltyService>();
        _sut = new ProcessReturnCommandHandler(_unitOfWork, _returnItemValidator, _refundCalculator, _customerLoyaltyService);
    }

    [Fact]
    public async Task Handle_WhenInvoiceNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var command = new ProcessReturnCommand(1, new ProcessReturnRequest([new ReturnItemRequest(1, 1)]), "user1");

        A.CallTo(() => _unitOfWork.Invoices.TrackedFindAsync(A<Expression<Func<Invoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(Task.FromResult<Invoice?>(null));

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ReturnsValidationError()
    {
        // Arrange — validator returns an error (product not in invoice)
        var command = new ProcessReturnCommand(1, new ProcessReturnRequest([new ReturnItemRequest(99, 1)]), "user1");

        var invoice = Invoice.Create(1, null, 100m, 0m, 1);
        invoice.Items.Add(InvoiceItem.Create(1, 1, 5, 20m));

        A.CallTo(() => _unitOfWork.Invoices.TrackedFindAsync(A<Expression<Func<Invoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(invoice);

        A.CallTo(() => _unitOfWork.Returns.FindAllAsync(A<Expression<Func<Return, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(new List<Return>());

        A.CallTo(() => _returnItemValidator.Validate(invoice, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(InvoiceErrors.ProductNotInInvoice);

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.ProductNotInInvoice, result.Error);
    }

    [Fact]
    public async Task Handle_WhenShiftNotOpen_ReturnsShiftNotOpenError()
    {
        // Arrange
        var command = new ProcessReturnCommand(1, new ProcessReturnRequest([new ReturnItemRequest(1, 2)]), "user1");

        var invoice = Invoice.Create(1, null, 100m, 0m, 1);
        invoice.Items.Add(InvoiceItem.Create(1, 1, 5, 20m));

        A.CallTo(() => _unitOfWork.Invoices.TrackedFindAsync(A<Expression<Func<Invoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(invoice);

        A.CallTo(() => _unitOfWork.Returns.FindAllAsync(A<Expression<Func<Return, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(new List<Return>());

        A.CallTo(() => _returnItemValidator.Validate(A<Invoice>._, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(null); // validation passes

        A.CallTo(() => _unitOfWork.Shifts.TrackedFindAsync(A<Expression<Func<Shift, bool>>>._, A<CancellationToken>._))
            .Returns(Task.FromResult<Shift?>(null));

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.ShiftNotOpen, result.Error);
    }

    [Fact]
    public async Task Handle_WhenDiscountRetained_RefundsCorrectly()
    {
        // Arrange
        var customerId = 5;
        var command = new ProcessReturnCommand(1, new ProcessReturnRequest([new ReturnItemRequest(1, 2)]), "user1");

        var invoice = Invoice.Create(1, customerId, 100m, 10m, 1);
        invoice.Items.Add(InvoiceItem.Create(1, 1, 5, 20m));

        var shift = Shift.Create("user1", 100m);
        shift.Id = 2;

        var customer = Customer.Create("Jane Doe", "0123");
        customer.Id = customerId;

        A.CallTo(() => _unitOfWork.Invoices.TrackedFindAsync(A<Expression<Func<Invoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(invoice);

        A.CallTo(() => _unitOfWork.Returns.FindAllAsync(A<Expression<Func<Return, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(new List<Return>());

        A.CallTo(() => _returnItemValidator.Validate(A<Invoice>._, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(null);

        A.CallTo(() => _unitOfWork.Shifts.TrackedFindAsync(A<Expression<Func<Shift, bool>>>._, A<CancellationToken>._))
            .Returns(shift);

        A.CallTo(() => _unitOfWork.Customers.GetAsync(A<object[]>._, A<CancellationToken>._))
            .Returns(customer);

        var inventory = Inventory.Create(1, 10, 20m);
        A.CallTo(() => _unitOfWork.Inventories.TrackedFindAllAsync(A<Expression<Func<Inventory, bool>>>._, A<CancellationToken>._))
            .Returns(new List<Inventory> { inventory });

        var itemRefunds = new Dictionary<int, decimal> { [1] = 36m };
        A.CallTo(() => _refundCalculator.CalculateRefundAsync(invoice, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemEntry>>._, customer, A<CancellationToken>._))
            .Returns(new ReturnRefundResult(36m, 0m, itemRefunds));

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(36m, result.Value.TotalRefund);
        Assert.Equal(0m, result.Value.DiscountClawback);
        Assert.Equal(12, inventory.Quantity); // 10 + 2 restored
        Assert.Equal(36m, shift.TotalReturns);
        A.CallTo(() => _customerLoyaltyService.UpdateCustomerLoyaltyAsync(customer, -36m, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.CommitTransactionAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_WhenDiscountLost_ReturnsClawbackAmount()
    {
        // Arrange
        var customerId = 5;
        var command = new ProcessReturnCommand(1, new ProcessReturnRequest([new ReturnItemRequest(1, 2)]), "user1");

        var invoice = Invoice.Create(1, customerId, 500m, 50m, 1);
        invoice.Items.Add(InvoiceItem.Create(1, 1, 5, 100m));

        var shift = Shift.Create("user1", 100m);
        shift.Id = 2;

        var customer = Customer.Create("Jane Doe", "0123");
        customer.Id = customerId;

        A.CallTo(() => _unitOfWork.Invoices.TrackedFindAsync(A<Expression<Func<Invoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(invoice);

        A.CallTo(() => _unitOfWork.Returns.FindAllAsync(A<Expression<Func<Return, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(new List<Return>());

        A.CallTo(() => _returnItemValidator.Validate(A<Invoice>._, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(null);

        A.CallTo(() => _unitOfWork.Shifts.TrackedFindAsync(A<Expression<Func<Shift, bool>>>._, A<CancellationToken>._))
            .Returns(shift);

        A.CallTo(() => _unitOfWork.Customers.GetAsync(A<object[]>._, A<CancellationToken>._))
            .Returns(customer);

        var inventory = Inventory.Create(1, 10, 100m);
        A.CallTo(() => _unitOfWork.Inventories.TrackedFindAllAsync(A<Expression<Func<Inventory, bool>>>._, A<CancellationToken>._))
            .Returns(new List<Inventory> { inventory });

        var itemRefunds = new Dictionary<int, decimal> { [1] = 150m };
        A.CallTo(() => _refundCalculator.CalculateRefundAsync(invoice, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemEntry>>._, customer, A<CancellationToken>._))
            .Returns(new ReturnRefundResult(150m, 30m, itemRefunds));

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(150m, result.Value.TotalRefund);
        Assert.Equal(30m, result.Value.DiscountClawback);
        A.CallTo(() => _customerLoyaltyService.UpdateCustomerLoyaltyAsync(customer, -150m, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _unitOfWork.CommitTransactionAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
