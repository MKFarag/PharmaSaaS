using Mapster;
using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Domain.Entities;
using System.Linq.Expressions;

namespace PharmaSaaS.Application.Features.PurchaseOrders.List;

public sealed class ListPurchaseOrdersQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ListPurchaseOrdersQuery, Result<IPaginatedList<PurchaseOrderResponse>>>
{
    public async Task<Result<IPaginatedList<PurchaseOrderResponse>>> Handle(ListPurchaseOrdersQuery query, CancellationToken cancellationToken = default)
    {
        Expression<Func<PurchaseOrder, bool>> predicate = po =>
            (!query.SupplierId.HasValue || po.SupplierId == query.SupplierId.Value) &&
            (!query.Date.HasValue || po.OrderDate == query.Date.Value);

        string[] includes = [nameof(PurchaseOrder.Supplier)];

        var purchaseOrders = await unitOfWork.PurchaseOrders.FindPaginatedListAsync<PurchaseOrderResponse>(
            predicate,
            query.PageNumber,
            query.PageSize,
            nameof(PurchaseOrder.Id),
            includes,
            cancellationToken);

        return Result.Success(purchaseOrders);
    }
}
