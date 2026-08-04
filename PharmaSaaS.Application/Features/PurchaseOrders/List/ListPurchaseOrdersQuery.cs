using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Domain.Abstraction;

namespace PharmaSaaS.Application.Features.PurchaseOrders.List;

public record ListPurchaseOrdersQuery(int? SupplierId, DateOnly? Date, int PageNumber = 1, int PageSize = 10) : IRequest<Result<IPaginatedList<PurchaseOrderResponse>>>;
