namespace PharmaSaaS.Application.Test.Invoices;

public class ProcessReturnCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IReturnService _returnService = A.Fake<IReturnService>();
    private readonly ICustomerLoyaltyService _customerLoyaltyService = A.Fake<ICustomerLoyaltyService>();
    private readonly ProcessReturnCommandHandler _sut;

    public ProcessReturnCommandHandlerTests()
        => _sut = new ProcessReturnCommandHandler(_unitOfWork, _returnService, _customerLoyaltyService);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ProcessReturnCommand DefaultCommand(int? customerId = null)
        => new(1, new ProcessReturnRequest([new ReturnItemRequest(1, 2)]));

    private void SetupInvoice(Invoice? invoice)
        => A.CallTo(() => _unitOfWork.Invoices.TrackedFindAsync(
                A<Expression<Func<Invoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(invoice);

    private void SetupShift(Shift? shift)
        => A.CallTo(() => _unitOfWork.Shifts.TrackedFindAsync(
                A<Expression<Func<Shift, bool>>>._, A<CancellationToken>._))
            .Returns(shift);

    // ── Invoice not found ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenInvoiceNotFound_ReturnsNotFoundError()
    {
        SetupInvoice(null);

        var result = await _sut.Handle(DefaultCommand(), default);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.NotFound, result.Error);
    }

    // ── Validation failure ────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenValidationFails_ReturnsValidationError()
    {
        var invoice = Invoice.Create(1, null, 100m, 0m, 1);
        invoice.Items.Add(InvoiceItem.Create(1, 1, 5, 20m));
        SetupInvoice(invoice);

        A.CallTo(() => _returnService.ValidateReturnRequest(invoice, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(InvoiceErrors.ProductNotInInvoice);

        var result = await _sut.Handle(DefaultCommand(), default);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.ProductNotInInvoice, result.Error);
    }

    // ── Shift not open ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenShiftNotOpen_ReturnsShiftNotOpenError()
    {
        var invoice = Invoice.Create(1, null, 100m, 0m, 1);
        invoice.Items.Add(InvoiceItem.Create(1, 1, 5, 20m));
        SetupInvoice(invoice);

        A.CallTo(() => _returnService.ValidateReturnRequest(A<Invoice>._, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(null);

        SetupShift(null);

        var result = await _sut.Handle(DefaultCommand(), default);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.ShiftNotOpen, result.Error);
    }

    // ── Happy path — discount retained ───────────────────────────────────────

    [Fact]
    public async Task Handle_WhenDiscountRetained_RefundsCorrectly()
    {
        var customerId = 5;
        var invoice = Invoice.Create(1, customerId, 100m, 10m, 1);
        invoice.Items.Add(InvoiceItem.Create(1, 1, 5, 20m));
        SetupInvoice(invoice);

        A.CallTo(() => _returnService.ValidateReturnRequest(A<Invoice>._, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(null);

        var shift = Shift.Create("user1", 100m);
        shift.Id = 2;
        SetupShift(shift);

        var entries = new[] { new ReturnItemEntry(1, 2, 20m) };
        A.CallTo(() => _returnService.BuildEntries(A<Invoice>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(entries);

        var itemRefunds = new Dictionary<int, decimal> { [1] = 36m };
        A.CallTo(() => _returnService.CalculateRefundAsync(A<Invoice>._, A<IEnumerable<Return>>._, entries, A<Customer?>._, A<CancellationToken>._))
            .Returns(new ReturnRefundResult(36m, 0m, itemRefunds));

        var inventory = Inventory.Create(1, 10, 20m);
        A.CallTo(() => _unitOfWork.Inventories.TrackedFindAllAsync(A<Expression<Func<Inventory, bool>>>._, A<CancellationToken>._))
            .Returns(new List<Inventory> { inventory });

        var result = await _sut.Handle(DefaultCommand(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(36m, result.Value.TotalRefund);
        Assert.Equal(0m, result.Value.DiscountClawback);
        Assert.Equal(12, inventory.Quantity); // 10 + 2 restored
        Assert.Equal(36m, shift.TotalReturns);
        A.CallTo(() => _unitOfWork.CommitTransactionAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    // ── Happy path — discount lost ────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenDiscountLost_ReturnsClawbackAmount()
    {
        var customerId = 5;
        var invoice = Invoice.Create(1, customerId, 500m, 50m, 1);
        invoice.Items.Add(InvoiceItem.Create(1, 1, 5, 100m));
        SetupInvoice(invoice);

        A.CallTo(() => _returnService.ValidateReturnRequest(A<Invoice>._, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(null);

        var shift = Shift.Create("user1", 100m);
        shift.Id = 2;
        SetupShift(shift);

        var entries = new[] { new ReturnItemEntry(1, 2, 100m) };
        A.CallTo(() => _returnService.BuildEntries(A<Invoice>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(entries);

        var itemRefunds = new Dictionary<int, decimal> { [1] = 150m };
        A.CallTo(() => _returnService.CalculateRefundAsync(A<Invoice>._, A<IEnumerable<Return>>._, entries, A<Customer?>._, A<CancellationToken>._))
            .Returns(new ReturnRefundResult(150m, 30m, itemRefunds));

        A.CallTo(() => _unitOfWork.Inventories.TrackedFindAllAsync(A<Expression<Func<Inventory, bool>>>._, A<CancellationToken>._))
            .Returns(new List<Inventory>());

        var result = await _sut.Handle(DefaultCommand(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(150m, result.Value.TotalRefund);
        Assert.Equal(30m, result.Value.DiscountClawback);
        A.CallTo(() => _unitOfWork.CommitTransactionAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
