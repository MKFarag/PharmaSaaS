namespace PharmaSaaS.Application.Features.Inventories.GetAllInventories;

public sealed class GetAllInventoriesQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllInventoriesQuery, IPaginatedList<InventoryResponse>>
{
    public async Task<IPaginatedList<InventoryResponse>> Handle(GetAllInventoriesQuery request, CancellationToken cancellationToken = default)
    {
        var f = request.Filters;

        Expression<Func<Inventory, bool>> predicate = i =>
            (!f.IsRare.HasValue || i.IsRare == f.IsRare.Value) && (f.OutOfStock != true || i.Quantity == 0) &&
            (!f.BelowTarget.HasValue || 
                (i.TargetStock.HasValue && 
                    ((f.BelowTarget == true  && i.Quantity < i.TargetStock.Value) || (f.BelowTarget == false && i.Quantity >= i.TargetStock.Value))));

        return await unitOfWork.Inventories.FindPaginatedListAsync<InventoryResponse>(
            predicate,
            f.PageNumber, f.PageSize,
            f.SearchValue, nameof(Product.Name),
            nameof(Inventory.Id), OrderBy.Ascending,
            ColumnType.String,
            [
                nameof(Inventory.Product),
                $"{nameof(Inventory.Product)}.{nameof(Product.Category)}",
                $"{nameof(Inventory.Product)}.{nameof(Product.DosageForm)}"
            ],
            cancellationToken);
    }
}
