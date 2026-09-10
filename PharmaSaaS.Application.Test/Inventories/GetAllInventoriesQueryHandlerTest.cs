namespace PharmaSaaS.Application.Test.InventoriesTests;

public class GetAllInventoriesQueryHandlerTest
{
    private readonly IUnitOfWork _unitOfWork = A.Fake<IUnitOfWork>();
    private readonly IGenericRepositoryWithPagination<Inventory> _inventoryRepo = A.Fake<IGenericRepositoryWithPagination<Inventory>>();
    private readonly IInventoryFilterService _filterService = A.Fake<IInventoryFilterService>();
    private readonly GetAllInventoriesQueryHandler _handler;

    public GetAllInventoriesQueryHandlerTest()
    {
        A.CallTo(() => _unitOfWork.Inventories).Returns(_inventoryRepo);
        _handler = new GetAllInventoriesQueryHandler(_unitOfWork, _filterService);
    }

    [Fact]
    public async Task Handle_WhenCalled_DelegatesToFilterServiceAndRepository()
    {
        // Arrange
        Expression<Func<Inventory, bool>> predicate = _ => true;
        var filters = new InventoryRequestFilters();

        A.CallTo(() => _filterService.BuildPredicate(filters)).Returns(predicate);
        A.CallTo(() => _filterService.ResolveSorting(filters)).Returns((nameof(Inventory.Id), OrderBy.Ascending));

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

        // Act
        var result = await _handler.Handle(new GetAllInventoriesQuery(filters), CancellationToken.None);

        // Assert
        Assert.Equal(paged, result);

        A.CallTo(() => _filterService.BuildPredicate(filters)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _filterService.ResolveSorting(filters)).MustHaveHappenedOnceExactly();
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
