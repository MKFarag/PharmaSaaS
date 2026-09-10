namespace PharmaSaaS.Application.Features.PurchaseOrders.Get;

public sealed class GetPurchaseOrderQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetPurchaseOrderQuery, Result<PurchaseOrderDetailResponse>>
{
    public async Task<Result<PurchaseOrderDetailResponse>> Handle(GetPurchaseOrderQuery query, CancellationToken cancellationToken = default)
    {
        string[] includes =
        [
            nameof(PurchaseOrder.Supplier),
            nameof(PurchaseOrder.Items),
            $"{nameof(PurchaseOrder.Items)}.{nameof(PurchaseOrderItem.Product)}"
        ];

        if (await unitOfWork.PurchaseOrders.FindAsync(po => po.Id == query.Id, includes, cancellationToken) is not { } purchaseOrder)
            return Result.Failure<PurchaseOrderDetailResponse>(PurchaseOrderErrors.NotFound);

        return Result.Success(purchaseOrder.Adapt<PurchaseOrderDetailResponse>());
    }
}
