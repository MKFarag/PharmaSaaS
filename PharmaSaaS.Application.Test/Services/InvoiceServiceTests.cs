namespace PharmaSaaS.Application.Test.Services;

public class InvoiceServiceTests
{
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _service = new InvoiceService();
    }

    [Fact]
    public void CalculateTotalAndValidateStock_ShouldReturnSuccessWithTotal_WhenStockIsSufficient()
    {
        // Arrange
        var requestedItems = new List<InvoiceItemRequest>
        {
            new(1, 2f), // ProductId=1, Qty=2
            new(2, 3f)  // ProductId=2, Qty=3
        };

        var inventories = new List<Inventory>
        {
            CreateInventory(1, 10, 50m), // Qty=10, SellingPrice=50
            CreateInventory(2, 5, 30m)   // Qty=5, SellingPrice=30
        };

        // Act
        var result = _service.CalculateTotalAndValidateStock(requestedItems, inventories);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(190m, result.Value);
    }

    [Fact]
    public void CalculateTotalAndValidateStock_ShouldReturnFailure_WhenProductNotFoundInInventory()
    {
        // Arrange
        var requestedItems = new List<InvoiceItemRequest>
        {
            new(3, 1f)
        };

        var inventories = new List<Inventory>
        {
            CreateInventory(1, 10, 50m)
        };

        // Act
        var result = _service.CalculateTotalAndValidateStock(requestedItems, inventories);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invoice.ProductNotFound", result.Error.Code);
    }

    [Fact]
    public void CalculateTotalAndValidateStock_ShouldReturnFailure_WhenStockIsInsufficient()
    {
        // Arrange
        var requestedItems = new List<InvoiceItemRequest>
        {
            new(1, 20f)
        };

        var inventories = new List<Inventory>
        {
            CreateInventory(1, 10, 50m)
        };

        // Act
        var result = _service.CalculateTotalAndValidateStock(requestedItems, inventories);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invoice.InsufficientStock", result.Error.Code);
    }

    [Fact]
    public void ProcessInvoiceItemsAndDeductStock_ShouldDeductStockAndReturnInvoiceItems()
    {
        // Arrange
        var requestedItems = new List<InvoiceItemRequest>
        {
            new(1, 2f)
        };

        var inventories = new List<Inventory>
        {
            CreateInventory(1, 10, 50m)
        };

        // Act
        var items = _service.ProcessInvoiceItemsAndDeductStock(99, requestedItems, inventories);

        // Assert
        Assert.Single(items);
        Assert.Equal(99, items[0].InvoiceId);
        Assert.Equal(1, items[0].ProductId);
        Assert.Equal(2, items[0].Quantity);
        Assert.Equal(50m, items[0].UnitPrice);

        // Verify domain behavior on inventory
        Assert.Equal(8, inventories[0].Quantity);
        Assert.Equal(2, inventories[0].TotalUnitsSold);
    }

    private static Inventory CreateInventory(int productId, int quantity, decimal sellingPrice)
    {
        var inv = Inventory.Create(productId, quantity, 10); // targetStock=10
        inv.SetSellingPrice(sellingPrice);
        return inv;
    }
}
