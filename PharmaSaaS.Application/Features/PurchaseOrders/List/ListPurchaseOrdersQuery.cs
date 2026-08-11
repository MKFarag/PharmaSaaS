using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Domain.Abstraction;

using PharmaSaaS.Application.Contracts.PurchaseOrders;

namespace PharmaSaaS.Application.Features.PurchaseOrders.List;

public record ListPurchaseOrdersQuery(PurchaseOrderRequestFilters Filters) : IRequest<IPaginatedList<PurchaseOrderResponse>>;
