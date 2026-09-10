namespace PharmaSaaS.Application.Features.Inventories.GetAllInventories;

public sealed class GetAllInventoriesQueryHandler(IUnitOfWork unitOfWork, IInventoryFilterService filterService)
    : IRequestHandler<GetAllInventoriesQuery, IPaginatedList<InventoryResponse>>
{
    public async Task<IPaginatedList<InventoryResponse>> Handle(GetAllInventoriesQuery request, CancellationToken cancellationToken = default)
    {
        var predicate = filterService.BuildPredicate(request.Filters);
        var (sortColumn, sortDirection) = filterService.ResolveSorting(request.Filters);

        return await unitOfWork.Inventories.FindPaginatedListAsync<InventoryResponse>(
            predicate,
            request.Filters.PageNumber, request.Filters.PageSize,
            request.Filters.ProductName, $"{nameof(Inventory.Product)}.{nameof(Product.Name)}",
            sortColumn, sortDirection,
            ColumnType.String,
            [nameof(Inventory.Product)],
            cancellationToken);
    }
}
