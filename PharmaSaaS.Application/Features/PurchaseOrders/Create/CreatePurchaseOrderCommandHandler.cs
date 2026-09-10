namespace PharmaSaaS.Application.Features.PurchaseOrders.Create;

public sealed class CreatePurchaseOrderCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreatePurchaseOrderCommand, Result<PurchaseOrderResponse>>
{
    public async Task<Result<PurchaseOrderResponse>> Handle(CreatePurchaseOrderCommand command, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Suppliers.GetAsync([command.Request.SupplierId], cancellationToken) is not { } supplier)
            return Result.Failure<PurchaseOrderResponse>(SupplierErrors.NotFound);

        var productIds = command.Request.Items.Select(x => x.ProductId).Distinct().ToList();
        var productsCount = await unitOfWork.Products.CountAsync(p => productIds.Contains(p.Id), cancellationToken);

        if (productsCount != productIds.Count)
            return Result.Failure<PurchaseOrderResponse>(ProductErrors.NotFound);

        decimal totalAmount = command.Request.ExpectedTotalAmount ?? 0;

        var purchaseOrder = PurchaseOrder.Create(command.Request.SupplierId, totalAmount, 0);

        await unitOfWork.PurchaseOrders.AddAsync(purchaseOrder, cancellationToken);

        purchaseOrder.SetItems([.. command.Request.Items.Select(item =>
            PurchaseOrderItem.Create(item.ProductId, item.Quantity, 0)
        )]);

        await unitOfWork.CompleteAsync(cancellationToken);

        purchaseOrder.SetSupplier(supplier);

        return Result.Success(purchaseOrder.Adapt<PurchaseOrderResponse>());
    }
}
