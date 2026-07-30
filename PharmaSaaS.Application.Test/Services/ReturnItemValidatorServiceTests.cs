using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Application.Services;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;
using Xunit;

namespace PharmaSaaS.Application.Test.Services;

public class ReturnItemValidatorServiceTests
{
    private readonly ReturnItemValidatorService _sut = new();

    private static Invoice MakeInvoice(params (int ProductId, float Qty, decimal Price)[] items)
    {
        var invoice = Invoice.Create(1, null, 100m, 0m, items.Length);
        foreach (var (pid, qty, price) in items)
            invoice.Items.Add(InvoiceItem.Create(invoice.Id, pid, qty, price));
        return invoice;
    }

    [Fact]
    public void Validate_WhenAllItemsValidAndNoExistingReturns_ReturnsNull()
    {
        // Arrange
        var invoice = MakeInvoice((1, 5, 20m));
        var request = new[] { new ReturnItemRequest(1, 2) };

        // Act
        var error = _sut.Validate(invoice, [], request);

        // Assert
        Assert.Null(error);
    }

    [Fact]
    public void Validate_WhenProductNotInInvoice_ReturnsProductNotInInvoiceError()
    {
        // Arrange
        var invoice = MakeInvoice((1, 5, 20m)); // only product 1
        var request = new[] { new ReturnItemRequest(99, 1) }; // requesting product 99

        // Act
        var error = _sut.Validate(invoice, [], request);

        // Assert
        Assert.Equal(InvoiceErrors.ProductNotInInvoice, error);
    }

    [Fact]
    public void Validate_WhenQuantityExceedsPurchased_ReturnsReturnQuantityExceededError()
    {
        // Arrange
        var invoice = MakeInvoice((1, 3, 20m)); // bought 3
        var request = new[] { new ReturnItemRequest(1, 5) }; // trying to return 5

        // Act
        var error = _sut.Validate(invoice, [], request);

        // Assert
        Assert.Equal(InvoiceErrors.ReturnQuantityExceeded, error);
    }

    [Fact]
    public void Validate_WhenExistingReturnsMakeQuantityExceed_ReturnsReturnQuantityExceededError()
    {
        // Arrange
        var invoice = MakeInvoice((1, 5, 20m)); // bought 5
        var existingReturn = Return.Create(1, 1, 4, 80m); // already returned 4
        var request = new[] { new ReturnItemRequest(1, 2) }; // trying to return 2 more → total 6 > 5

        // Act
        var error = _sut.Validate(invoice, [existingReturn], request);

        // Assert
        Assert.Equal(InvoiceErrors.ReturnQuantityExceeded, error);
    }

    [Fact]
    public void Validate_WhenExactlyRemainingQuantityRequested_ReturnsNull()
    {
        // Arrange
        var invoice = MakeInvoice((1, 5, 20m)); // bought 5
        var existingReturn = Return.Create(1, 1, 2, 40m); // already returned 2
        var request = new[] { new ReturnItemRequest(1, 3) }; // returning remaining 3 → total 5 = 5 ✓

        // Act
        var error = _sut.Validate(invoice, [existingReturn], request);

        // Assert
        Assert.Null(error);
    }

    [Fact]
    public void Validate_WhenMultipleItemsAndOneInvalid_ReturnsErrorForInvalidItem()
    {
        // Arrange
        var invoice = MakeInvoice((1, 5, 20m), (2, 3, 30m));
        var request = new[] { new ReturnItemRequest(1, 1), new ReturnItemRequest(2, 10) }; // product 2 qty 10 > 3

        // Act
        var error = _sut.Validate(invoice, [], request);

        // Assert
        Assert.Equal(InvoiceErrors.ReturnQuantityExceeded, error);
    }
}
