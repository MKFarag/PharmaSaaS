using Mapster;
using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Domain.Entities;
using System.Linq.Expressions;

namespace PharmaSaaS.Application.Features.PurchaseOrders.List;

public sealed class ListPurchaseOrdersQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ListPurchaseOrdersQuery, IPaginatedList<PurchaseOrderResponse>>
{
    public async Task<IPaginatedList<PurchaseOrderResponse>> Handle(ListPurchaseOrdersQuery query, CancellationToken cancellationToken = default)
    {
        Expression<Func<PurchaseOrder, bool>> predicate = po =>
            (!query.Filters.SupplierId.HasValue || po.SupplierId == query.Filters.SupplierId.Value) &&
            (!query.Filters.Date.HasValue || po.OrderDate == query.Filters.Date.Value);

        string[] includes = [nameof(PurchaseOrder.Supplier), nameof(PurchaseOrder.Items)];

        var purchaseOrders = await unitOfWork.PurchaseOrders.FindPaginatedListAsync<PurchaseOrderResponse>(
            predicate,
            query.Filters.PageNumber,
            query.Filters.PageSize,
            nameof(PurchaseOrder.OrderDate),
            includes,
            cancellationToken);

        return purchaseOrders;
    }
}
