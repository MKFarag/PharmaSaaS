namespace PharmaSaaS.Application.Test.Invoices;

public class CheckReturnImpactQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IReturnService _returnService = A.Fake<IReturnService>();
    private readonly CheckReturnImpactQueryHandler _sut;

    public CheckReturnImpactQueryHandlerTests()
        => _sut = new CheckReturnImpactQueryHandler(_unitOfWork, _returnService);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static CheckReturnImpactQuery DefaultQuery(int invoiceId = 1)
        => new(invoiceId, new ProcessReturnRequest([new ReturnItemRequest(1, 2)]));

    private void SetupInvoice(Invoice? invoice)
        => A.CallTo(() => _unitOfWork.Invoices.FindAsync(
                A<Expression<Func<Invoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(invoice);

    private static readonly IReadOnlyList<ReturnItemEntry> EmptyEntries =
        [new ReturnItemEntry(1, 2, 20m)];

    private static readonly ReturnRefundResult ZeroRefund =
        new(0m, 0m, new Dictionary<int, decimal> { [1] = 0m });

    // ── Invoice not found ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenInvoiceNotFound_ReturnsNotFoundError()
    {
        SetupInvoice(null);

        var result = await _sut.Handle(DefaultQuery(), default);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.NotFound, result.Error);
        A.CallTo(() => _returnService.ValidateReturnRequest(A<Invoice>._, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .MustNotHaveHappened();
    }

    // ── Validation failure ────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenProductNotInInvoice_ReturnsProductNotInInvoiceError()
    {
        var invoice = Invoice.Create(1, null, 100m, 0m, 1);
        SetupInvoice(invoice);

        A.CallTo(() => _returnService.ValidateReturnRequest(invoice, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(InvoiceErrors.ProductNotInInvoice);

        var result = await _sut.Handle(DefaultQuery(), default);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.ProductNotInInvoice, result.Error);
        A.CallTo(() => _returnService.BuildEntries(A<Invoice>._, A<IEnumerable<ReturnItemRequest>>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenQuantityExceeded_ReturnsReturnQuantityExceededError()
    {
        var invoice = Invoice.Create(1, null, 100m, 0m, 1);
        SetupInvoice(invoice);

        A.CallTo(() => _returnService.ValidateReturnRequest(invoice, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(InvoiceErrors.ReturnQuantityExceeded);

        var result = await _sut.Handle(DefaultQuery(), default);

        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.ReturnQuantityExceeded, result.Error);
    }

    // ── Anonymous invoice (no customer) ──────────────────────────────────────

    [Fact]
    public async Task Handle_WhenAnonymousInvoice_ReturnsRefundWithNoTierProjection()
    {
        // invoice.Customer is null — no loyalty tier to assess
        var invoice = Invoice.Create(1, null, 200m, 0m, 1);
        SetupInvoice(invoice);

        A.CallTo(() => _returnService.ValidateReturnRequest(A<Invoice>._, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(null);
        A.CallTo(() => _returnService.BuildEntries(A<Invoice>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(EmptyEntries);

        var refund = new ReturnRefundResult(80m, 0m, new Dictionary<int, decimal> { [1] = 80m });
        A.CallTo(() => _returnService.CalculateRefundAsync(A<Invoice>._, A<IEnumerable<Return>>._, EmptyEntries, null, A<CancellationToken>._))
            .Returns(refund);

        var result = await _sut.Handle(DefaultQuery(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(80m, result.Value.EstimatedRefund);
        Assert.Equal(0m, result.Value.DiscountClawback);
        Assert.False(result.Value.WillDropBelowLoyaltyTier);
        Assert.Null(result.Value.CurrentTierName);
        Assert.Null(result.Value.CurrentTierThreshold);
        Assert.Equal(0m, result.Value.ProjectedSpendAfterReturn);
    }

    // ── Customer with no tier ─────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenCustomerHasNoTier_ReturnsRefundWithNoTierDrop()
    {
        // customer exists but has no CurrentTier assigned yet
        var customer = Customer.Create("Sara", "0501234567");
        customer.UpdateSpendAndTier(150m, null); // TotalSpend = 150, no tier

        var invoice = Invoice.Create(1, customer.Id, 150m, 0m, 1);
        invoice.Items.Add(InvoiceItem.Create(invoice.Id, 1, 3, 50m));

        // Simulate EF nav prop population
        typeof(Invoice)
            .GetProperty(nameof(Invoice.Customer))!
            .SetValue(invoice, customer);

        SetupInvoice(invoice);

        A.CallTo(() => _returnService.ValidateReturnRequest(A<Invoice>._, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(null);
        A.CallTo(() => _returnService.BuildEntries(A<Invoice>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(EmptyEntries);

        var refund = new ReturnRefundResult(50m, 0m, new Dictionary<int, decimal> { [1] = 50m });
        A.CallTo(() => _returnService.CalculateRefundAsync(A<Invoice>._, A<IEnumerable<Return>>._, EmptyEntries, customer, A<CancellationToken>._))
            .Returns(refund);

        var result = await _sut.Handle(DefaultQuery(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(50m, result.Value.EstimatedRefund);
        Assert.False(result.Value.WillDropBelowLoyaltyTier); // no tier → cannot drop below it
        Assert.Null(result.Value.CurrentTierName);
        Assert.Equal(100m, result.Value.ProjectedSpendAfterReturn); // 150 - 50
    }

    // ── Customer stays above tier threshold ───────────────────────────────────

    [Fact]
    public async Task Handle_WhenRefundKeepsCustomerAboveTierThreshold_ReturnsFalseForWillDrop()
    {
        // Tier threshold = 200. Customer spend = 500. Refund = 100. Projected = 400 > 200 → no drop.
        var tier = LoyaltyTier.Create("Gold", 200m);
        var customer = Customer.Create("Ali", "0509876543");
        customer.UpdateSpendAndTier(500m, 1);

        var invoice = Invoice.Create(1, customer.Id, 500m, 0m, 1);
        invoice.Items.Add(InvoiceItem.Create(invoice.Id, 1, 5, 100m));

        typeof(Customer).GetProperty(nameof(Customer.CurrentTier))!.SetValue(customer, tier);
        typeof(Invoice).GetProperty(nameof(Invoice.Customer))!.SetValue(invoice, customer);

        SetupInvoice(invoice);

        A.CallTo(() => _returnService.ValidateReturnRequest(A<Invoice>._, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(null);
        A.CallTo(() => _returnService.BuildEntries(A<Invoice>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(EmptyEntries);

        var refund = new ReturnRefundResult(100m, 0m, new Dictionary<int, decimal> { [1] = 100m });
        A.CallTo(() => _returnService.CalculateRefundAsync(A<Invoice>._, A<IEnumerable<Return>>._, EmptyEntries, customer, A<CancellationToken>._))
            .Returns(refund);

        var result = await _sut.Handle(DefaultQuery(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(100m, result.Value.EstimatedRefund);
        Assert.False(result.Value.WillDropBelowLoyaltyTier);
        Assert.Equal("Gold", result.Value.CurrentTierName);
        Assert.Equal(200m, result.Value.CurrentTierThreshold);
        Assert.Equal(400m, result.Value.ProjectedSpendAfterReturn); // 500 - 100
    }

    // ── Customer drops below tier threshold ───────────────────────────────────

    [Fact]
    public async Task Handle_WhenRefundDropsCustomerBelowTierThreshold_ReturnsTrueForWillDrop()
    {
        // Tier threshold = 300. Customer spend = 350. Refund = 200. Projected = 150 < 300 → drop!
        var tier = LoyaltyTier.Create("Silver", 300m);
        var customer = Customer.Create("Mona", "0551111111");
        customer.UpdateSpendAndTier(350m, 1);

        var invoice = Invoice.Create(1, customer.Id, 350m, 0m, 1);
        invoice.Items.Add(InvoiceItem.Create(invoice.Id, 1, 5, 70m));

        typeof(Customer).GetProperty(nameof(Customer.CurrentTier))!.SetValue(customer, tier);
        typeof(Invoice).GetProperty(nameof(Invoice.Customer))!.SetValue(invoice, customer);

        SetupInvoice(invoice);

        A.CallTo(() => _returnService.ValidateReturnRequest(A<Invoice>._, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(null);
        A.CallTo(() => _returnService.BuildEntries(A<Invoice>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(EmptyEntries);

        var refund = new ReturnRefundResult(200m, 15m, new Dictionary<int, decimal> { [1] = 200m });
        A.CallTo(() => _returnService.CalculateRefundAsync(A<Invoice>._, A<IEnumerable<Return>>._, EmptyEntries, customer, A<CancellationToken>._))
            .Returns(refund);

        var result = await _sut.Handle(DefaultQuery(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(200m, result.Value.EstimatedRefund);
        Assert.Equal(15m, result.Value.DiscountClawback);
        Assert.True(result.Value.WillDropBelowLoyaltyTier);
        Assert.Equal("Silver", result.Value.CurrentTierName);
        Assert.Equal(300m, result.Value.CurrentTierThreshold);
        Assert.Equal(150m, result.Value.ProjectedSpendAfterReturn); // 350 - 200
    }

    // ── Projected spend cannot go negative ────────────────────────────────────

    [Fact]
    public async Task Handle_WhenRefundExceedsCustomerSpend_ProjectedSpendIsZero()
    {
        // Refund (300) exceeds TotalSpend (200) → Math.Max(0, ...) clamps to 0.
        var tier = LoyaltyTier.Create("Bronze", 100m);
        var customer = Customer.Create("Youssef", "0522222222");
        customer.UpdateSpendAndTier(200m, 1);

        var invoice = Invoice.Create(1, customer.Id, 200m, 0m, 1);

        typeof(Customer).GetProperty(nameof(Customer.CurrentTier))!.SetValue(customer, tier);
        typeof(Invoice).GetProperty(nameof(Invoice.Customer))!.SetValue(invoice, customer);

        SetupInvoice(invoice);

        A.CallTo(() => _returnService.ValidateReturnRequest(A<Invoice>._, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(null);
        A.CallTo(() => _returnService.BuildEntries(A<Invoice>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(EmptyEntries);

        // Refund exceeds spend (edge case)
        var refund = new ReturnRefundResult(300m, 0m, new Dictionary<int, decimal> { [1] = 300m });
        A.CallTo(() => _returnService.CalculateRefundAsync(A<Invoice>._, A<IEnumerable<Return>>._, EmptyEntries, customer, A<CancellationToken>._))
            .Returns(refund);

        var result = await _sut.Handle(DefaultQuery(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, result.Value.ProjectedSpendAfterReturn);
        Assert.True(result.Value.WillDropBelowLoyaltyTier); // 0 < 100 → still drops below
    }

    // ── Service delegation ────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenSuccessful_DelegatesAllStepsToReturnServiceInOrder()
    {
        var invoice = Invoice.Create(1, null, 100m, 0m, 1);
        SetupInvoice(invoice);

        A.CallTo(() => _returnService.ValidateReturnRequest(A<Invoice>._, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(null);
        A.CallTo(() => _returnService.BuildEntries(A<Invoice>._, A<IEnumerable<ReturnItemRequest>>._))
            .Returns(EmptyEntries);
        A.CallTo(() => _returnService.CalculateRefundAsync(A<Invoice>._, A<IEnumerable<Return>>._, EmptyEntries, A<Customer?>._, A<CancellationToken>._))
            .Returns(ZeroRefund);

        await _sut.Handle(DefaultQuery(), default);

        A.CallTo(() => _returnService.ValidateReturnRequest(invoice, A<IEnumerable<Return>>._, A<IEnumerable<ReturnItemRequest>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _returnService.BuildEntries(invoice, A<IEnumerable<ReturnItemRequest>>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _returnService.CalculateRefundAsync(invoice, A<IEnumerable<Return>>._, EmptyEntries, A<Customer?>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
