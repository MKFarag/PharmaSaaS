using PharmaSaaS.Application.Features.Inventories.AdjustStock;

namespace PharmaSaaS.Application.Test.Inventories;

public class AdjustStockCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Inventory> _inventoryRepo = A.Fake<IGenericRepositoryWithPagination<Inventory>>();
    private readonly AdjustStockCommandHandler _handler;

    public AdjustStockCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Inventories).Returns(_inventoryRepo);
        _handler = new AdjustStockCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenInventoryNotFound_ReturnsNotFoundError()
    {
        // Arrange
        A.CallTo(() => _inventoryRepo.FindAsync(
                A<Expression<Func<Inventory, bool>>>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns((Inventory?)null);

        var command = new AdjustStockCommand(1, new AdjustStockRequest(5));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(InventoryErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenInsufficientStock_ReturnsInsufficientStockError()
    {
        // Arrange
        var inventory = Inventory.Create(1, 10f, 5.50m, 50);

        A.CallTo(() => _inventoryRepo.FindAsync(
                A<Expression<Func<Inventory, bool>>>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(inventory);

        var command = new AdjustStockCommand(1, new AdjustStockRequest(-15));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(InventoryErrors.InsufficientStock, result.Error);

        A.CallTo(() => _inventoryRepo.ExecuteUpdateAsync(
                A<Expression<Func<Inventory, bool>>>.Ignored,
                A<string>.Ignored,
                A<float>.Ignored,
                A<CancellationToken>.Ignored))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_WhenValidAdjustment_ExecutesUpdateAndReturnsSuccess()
    {
        // Arrange
        var inventory = Inventory.Create(1, 10f, 5.50m, 50);

        A.CallTo(() => _inventoryRepo.FindAsync(
                A<Expression<Func<Inventory, bool>>>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(inventory);

        var command = new AdjustStockCommand(1, new AdjustStockRequest(5));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        A.CallTo(() => _inventoryRepo.ExecuteUpdateAsync(
                A<Expression<Func<Inventory, bool>>>.Ignored,
                nameof(Inventory.Quantity),
                15f,
                A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
