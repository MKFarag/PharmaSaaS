namespace PharmaSaaS.Application.Features.PurchaseOrders.List;

public sealed class ListPurchaseOrdersQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<ListPurchaseOrdersQuery, IPaginatedList<PurchaseOrderResponse>>
{
    public async Task<IPaginatedList<PurchaseOrderResponse>> Handle(ListPurchaseOrdersQuery query, CancellationToken cancellationToken = default)
    {
        Expression<Func<PurchaseOrder, bool>> predicate = po =>
            (!query.Filters.SupplierId.HasValue || po.SupplierId == query.Filters.SupplierId.Value) &&
            (!query.Filters.Date.HasValue || po.OrderDate == query.Filters.Date.Value);

        var purchaseOrders = await unitOfWork.PurchaseOrders
            .FindPaginatedListAsync<PurchaseOrderResponse>
            (
                predicate,
                query.Filters.PageNumber,
                query.Filters.PageSize,
                nameof(PurchaseOrder.OrderDate),
                [nameof(PurchaseOrder.Supplier), nameof(PurchaseOrder.Items)],
                cancellationToken
            );

        return purchaseOrders;
    }
}
