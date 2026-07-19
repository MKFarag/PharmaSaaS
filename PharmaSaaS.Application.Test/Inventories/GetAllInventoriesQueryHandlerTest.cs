namespace PharmaSaaS.Application.Test.InventoriesTests;

public class GetAllInventoriesQueryHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Inventory> _inventoryRepo = A.Fake<IGenericRepositoryWithPagination<Inventory>>();
    private readonly GetAllInventoriesQueryHandler _handler;

    public GetAllInventoriesQueryHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Inventories).Returns(_inventoryRepo);
        _handler = new GetAllInventoriesQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_ReturnsPagedInventory()
    {
        var paged = A.Fake<IPaginatedList<InventoryResponse>>();

        A.CallTo(() => _inventoryRepo.FindPaginatedListAsync<InventoryResponse>(
                A<Expression<Func<Inventory, bool>>>.Ignored,
                A<int>.Ignored, A<int>.Ignored,
                A<string?>.Ignored, A<string>.Ignored,
                A<string>.Ignored!, A<string>.Ignored!,
                A<ColumnType>.Ignored,
                A<string[]>.Ignored,
                A<CancellationToken>.Ignored))
            .Returns(paged);

        var result = await _handler.Handle(
            new GetAllInventoriesQuery(new InventoryRequestFilters()),
            CancellationToken.None);

        Assert.Equal(paged, result);

        A.CallTo(() => _inventoryRepo.FindPaginatedListAsync<InventoryResponse>(
                A<Expression<Func<Inventory, bool>>>.Ignored,
                A<int>.Ignored, A<int>.Ignored,
                A<string?>.Ignored, A<string>.Ignored,
                A<string>.Ignored!, A<string>.Ignored!,
                A<ColumnType>.Ignored,
                A<string[]>.Ignored,
                A<CancellationToken>.Ignored))
            .MustHaveHappenedOnceExactly();
    }
}
