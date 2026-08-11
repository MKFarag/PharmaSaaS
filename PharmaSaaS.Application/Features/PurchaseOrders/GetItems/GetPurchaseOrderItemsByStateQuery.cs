using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Application.Abstractions.Messaging;

using PharmaSaaS.Domain.Enums;

namespace PharmaSaaS.Application.Features.PurchaseOrders.GetItems;

public record GetPurchaseOrderItemsByStateQuery(int PurchaseOrderId, PurchaseOrderItemStatus State) : IRequest<Result<List<PurchaseOrderItemResponse>>>;
