namespace PharmaSaaS.Application.Features.PurchaseOrders.List;

public record ListPurchaseOrdersQuery(PurchaseOrderRequestFilters Filters) : IRequest<IPaginatedList<PurchaseOrderResponse>>;
