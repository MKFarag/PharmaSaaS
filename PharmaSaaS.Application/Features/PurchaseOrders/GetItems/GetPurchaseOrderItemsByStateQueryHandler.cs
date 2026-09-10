namespace PharmaSaaS.Application.Features.PurchaseOrders.GetItems;

public sealed class GetPurchaseOrderItemsByStateQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetPurchaseOrderItemsByStateQuery, Result<List<PurchaseOrderItemResponse>>>
{
    public async Task<Result<List<PurchaseOrderItemResponse>>> Handle(GetPurchaseOrderItemsByStateQuery query, CancellationToken cancellationToken = default)
    {
        string[] includes = [nameof(PurchaseOrder.Items), $"{nameof(PurchaseOrder.Items)}.{nameof(PurchaseOrderItem.Product)}"];

        if (await unitOfWork.PurchaseOrders.FindAsync(po => po.Id == query.PurchaseOrderId, includes, cancellationToken) is not { } purchaseOrder)
            return Result.Failure<List<PurchaseOrderItemResponse>>(PurchaseOrderErrors.NotFound);

        var response = purchaseOrder.Items
            .Where(i => i.Status == query.State)
            .Select(item => new PurchaseOrderItemResponse
            (
                item.Id,
                item.ProductId,
                item.Product.Name,
                item.Quantity,
                item.UnitCost,
                item.ReceivedQuantity,
                item.ReturnedQuantity,
                item.ReturnReason,
                item.Status.ToString()
            )).ToList();

        return Result.Success(response);
    }
}
