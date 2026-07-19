namespace PharmaSaaS.Application.Test.InventoriesTests;

public class UpdateStockCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Inventory> _inventoryRepo = A.Fake<IGenericRepositoryWithPagination<Inventory>>();
    private readonly UpdateStockCommandHandler _handler;

    public UpdateStockCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Inventories).Returns(_inventoryRepo);
        _handler = new UpdateStockCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenInventoryNotFound_ReturnsNotFoundError()
    {
        A.CallTo(() => _inventoryRepo.FindAsync(
                A<Expression<Func<Inventory, bool>>>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(Task.FromResult<Inventory?>(null));

        var command = new UpdateStockCommand(1, new UpdateStockRequest(100, 5.50m, 50, false));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(InventoryErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_ReturnsSuccess()
    {
        var inventory = Inventory.Create(1, 50f, 4.00m, 30);

        A.CallTo(() => _inventoryRepo.FindAsync(
                A<Expression<Func<Inventory, bool>>>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(Task.FromResult<Inventory?>(inventory));

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        var command = new UpdateStockCommand(1, new UpdateStockRequest(100, 5.50m, 50, true));

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(100f, inventory.Quantity);
        Assert.Equal(5.50m, inventory.PurchasePrice);
        Assert.Equal(50, inventory.TargetStock);
        Assert.True(inventory.IsRare);

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
