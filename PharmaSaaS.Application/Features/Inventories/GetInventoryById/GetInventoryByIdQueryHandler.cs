namespace PharmaSaaS.Application.Features.Inventories.GetInventoryById;

public sealed class GetInventoryByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetInventoryByIdQuery, Result<InventoryDetailResponse>>
{
    public async Task<Result<InventoryDetailResponse>> Handle(GetInventoryByIdQuery query, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Inventories.FindAsync(
                i => i.ProductId == query.ProductId,
                [
                    nameof(Inventory.Product),
                    $"{nameof(Inventory.Product)}.{nameof(Product.Category)}",
                    $"{nameof(Inventory.Product)}.{nameof(Product.DosageForm)}"
                ],
                cancellationToken) is not { } inventory)
            return Result.Failure<InventoryDetailResponse>(InventoryErrors.NotFound);

        return Result.Success(inventory.Adapt<InventoryDetailResponse>());
    }
}
