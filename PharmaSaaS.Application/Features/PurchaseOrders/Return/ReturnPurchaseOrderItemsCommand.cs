using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Application.Abstractions.Messaging;

namespace PharmaSaaS.Application.Features.PurchaseOrders.Return;

public record ReturnPurchaseOrderItemsCommand(int Id, ReturnPurchaseOrderItemsRequest Request) : IRequest<Result>;
