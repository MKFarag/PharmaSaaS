namespace PharmaSaaS.Application.Test.InventoriesTests;

public class ToggleRareCommandHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Inventory> _inventoryRepo = A.Fake<IGenericRepositoryWithPagination<Inventory>>();
    private readonly ToggleRareCommandHandler _handler;

    public ToggleRareCommandHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Inventories).Returns(_inventoryRepo);
        _handler = new ToggleRareCommandHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenInventoryNotFound_ReturnsNotFoundError()
    {
        A.CallTo(() => _inventoryRepo.FindAsync(
                A<Expression<Func<Inventory, bool>>>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(Task.FromResult<Inventory?>(null));

        var result = await _handler.Handle(new ToggleRareCommand(1), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(InventoryErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_ValidData_TogglesIsRare()
    {
        var inventory = Inventory.Create(1, 100, 5.50m, 50);
        Assert.False(inventory.IsRare);

        A.CallTo(() => _inventoryRepo.FindAsync(
                A<Expression<Func<Inventory, bool>>>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(Task.FromResult<Inventory?>(inventory));

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .Returns(1);

        var result = await _handler.Handle(new ToggleRareCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(inventory.IsRare);

        A.CallTo(() => _unitOfWork.CompleteAsync(A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
