namespace PharmaSaaS.Application.Test.Services;

public class ReturnServiceTests
{
    private readonly ICustomerLoyaltyService _loyaltyService = A.Fake<ICustomerLoyaltyService>();
    private readonly ReturnService _sut;

    public ReturnServiceTests() => _sut = new ReturnService(_loyaltyService);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Invoice MakeInvoice(decimal total, decimal discount, params (int ProductId, float Qty, decimal Price)[] items)
    {
        var invoice = Invoice.Create(1, null, total, discount, items.Length);
        foreach (var (pid, qty, price) in items)
            invoice.Items.Add(InvoiceItem.Create(invoice.Id, pid, qty, price));
        return invoice;
    }

    // ── ValidateReturnRequest ──────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WhenAllItemsValidAndNoExistingReturns_ReturnsNull()
    {
        var invoice = MakeInvoice(100m, 0m, (1, 5, 20m));
        var error = _sut.ValidateReturnRequest(invoice, [], [new ReturnItemRequest(1, 2)]);
        Assert.Null(error);
    }

    [Fact]
    public void Validate_WhenProductNotInInvoice_ReturnsProductNotInInvoiceError()
    {
        var invoice = MakeInvoice(100m, 0m, (1, 5, 20m));
        var error = _sut.ValidateReturnRequest(invoice, [], [new ReturnItemRequest(99, 1)]);
        Assert.Equal(InvoiceErrors.ProductNotInInvoice, error);
    }

    [Fact]
    public void Validate_WhenQuantityExceedsPurchased_ReturnsReturnQuantityExceededError()
    {
        var invoice = MakeInvoice(100m, 0m, (1, 3, 20m));
        var error = _sut.ValidateReturnRequest(invoice, [], [new ReturnItemRequest(1, 5)]);
        Assert.Equal(InvoiceErrors.ReturnQuantityExceeded, error);
    }

    [Fact]
    public void Validate_WhenExistingReturnsMakeQuantityExceed_ReturnsReturnQuantityExceededError()
    {
        var invoice = MakeInvoice(100m, 0m, (1, 5, 20m));
        var existingReturn = Return.Create(1, 1, 4, 80m); // already returned 4
        var error = _sut.ValidateReturnRequest(invoice, [existingReturn], [new ReturnItemRequest(1, 2)]); // 4 + 2 > 5
        Assert.Equal(InvoiceErrors.ReturnQuantityExceeded, error);
    }

    [Fact]
    public void Validate_WhenExactlyRemainingQuantityRequested_ReturnsNull()
    {
        var invoice = MakeInvoice(100m, 0m, (1, 5, 20m));
        var existingReturn = Return.Create(1, 1, 2, 40m); // returned 2
        var error = _sut.ValidateReturnRequest(invoice, [existingReturn], [new ReturnItemRequest(1, 3)]); // 2 + 3 == 5
        Assert.Null(error);
    }

    [Fact]
    public void Validate_WhenMultipleItemsAndOneInvalid_ReturnsErrorForInvalidItem()
    {
        var invoice = MakeInvoice(100m, 0m, (1, 5, 20m), (2, 3, 30m));
        var error = _sut.ValidateReturnRequest(invoice, [], [new ReturnItemRequest(1, 1), new ReturnItemRequest(2, 10)]);
        Assert.Equal(InvoiceErrors.ReturnQuantityExceeded, error);
    }

    // ── BuildEntries ────────────────────────────────────────────────

    [Fact]
    public void BuildReturnItemEntries_ResolvesUnitPriceFromInvoice()
    {
        var invoice = MakeInvoice(100m, 0m, (1, 5, 25m));
        var entries = _sut.BuildEntries(invoice, [new ReturnItemRequest(1, 2)]);

        Assert.Single(entries);
        Assert.Equal(1, entries[0].ProductId);
        Assert.Equal(2m, entries[0].Quantity);
        Assert.Equal(25m, entries[0].UnitPrice);
    }

    // ── CalculateRefundAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task CalculateRefundAsync_WhenNoCustomer_ReturnsFullBaseRefundWithNoClawback()
    {
        // Invoice: 200 total, 0 discount, paid 200. Returning 2 × 50 = 100 base.
        var invoice = MakeInvoice(200m, 0m);
        var entries = new[] { new ReturnItemEntry(1, 2, 50m) };

        var result = await _sut.CalculateRefundAsync(invoice, [], entries, null);

        Assert.Equal(100m, result.TotalRefund);
        Assert.Equal(0m, result.DiscountClawback);
        Assert.Equal(100m, result.ItemRefunds[1]);
        A.CallTo(() => _loyaltyService.CalculateDiscountAsync(A<Customer>._, A<decimal>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task CalculateRefundAsync_WhenCustomerStillAboveThreshold_RetainsDiscount()
    {
        // Invoice: 1000 total, 100 discount (10%), paid 900. Returning 1 × 100.
        // Remaining 900 still qualifies → new discount = 90. Refund = 900 − 0 − (900 − 90) = 90.
        var invoice = MakeInvoice(1000m, 100m);
        var customer = Customer.Create("John", "123");
        var entries = new[] { new ReturnItemEntry(1, 1, 100m) };

        A.CallTo(() => _loyaltyService.CalculateDiscountAsync(customer, 900m, A<CancellationToken>._)).Returns(90m);

        var result = await _sut.CalculateRefundAsync(invoice, [], entries, customer);

        Assert.Equal(90m, result.TotalRefund);
        Assert.Equal(0m, result.DiscountClawback);
    }

    [Fact]
    public async Task CalculateRefundAsync_WhenCustomerDropsBelowThreshold_ClawsBackDiscount()
    {
        // Invoice: 500 total, 50 discount (10%), paid 450. Returning 200 base.
        // Remaining 300 no longer qualifies → discount = 0. Refund = 450 − 0 − 300 = 150.
        var invoice = MakeInvoice(500m, 50m);
        var customer = Customer.Create("Jane", "456");
        var entries = new[] { new ReturnItemEntry(1, 1, 200m) };

        A.CallTo(() => _loyaltyService.CalculateDiscountAsync(customer, 300m, A<CancellationToken>._)).Returns(0m);

        var result = await _sut.CalculateRefundAsync(invoice, [], entries, customer);

        Assert.Equal(150m, result.TotalRefund);
        Assert.True(result.DiscountClawback > 0m);
    }

    [Fact]
    public async Task CalculateRefundAsync_WhenFullInvoiceReturned_RefundsEntireAmountPaid()
    {
        var invoice = MakeInvoice(300m, 0m);
        var customer = Customer.Create("Mira", "111");
        var entries = new[]
        {
            new ReturnItemEntry(1, 1, 100m),
            new ReturnItemEntry(2, 1, 100m),
            new ReturnItemEntry(3, 1, 100m)
        };

        A.CallTo(() => _loyaltyService.CalculateDiscountAsync(customer, 0m, A<CancellationToken>._)).Returns(0m);

        var result = await _sut.CalculateRefundAsync(invoice, [], entries, customer);

        Assert.Equal(300m, result.TotalRefund);
        Assert.Equal(0m, result.DiscountClawback);
        Assert.Equal(3, result.ItemRefunds.Count);
    }

    [Fact]
    public async Task CalculateRefundAsync_WhenPriorReturnsExist_AccountsForAlreadyRefunded()
    {
        // Invoice: 600 total, 60 discount (10%), paid 540. priorReturn refund = 180 (200 base × 0.9).
        // Returning 200 base now. remaining = 600 − 200 − 200 = 200. discount(200) = 0.
        // Refund = 540 − 180 − 200 = 160.
        var invoice = MakeInvoice(600m, 60m);
        var customer = Customer.Create("Ali", "789");
        var priorReturn = Return.Create(invoice.Id, 2, 1, 180m);
        var entries = new[] { new ReturnItemEntry(1, 1, 200m) };

        A.CallTo(() => _loyaltyService.CalculateDiscountAsync(customer, A<decimal>._, A<CancellationToken>._)).Returns(0m);

        var result = await _sut.CalculateRefundAsync(invoice, [priorReturn], entries, customer);

        Assert.Equal(160m, result.TotalRefund);
        Assert.True(result.ItemRefunds.ContainsKey(1));
    }
}
