using PharmaSaaS.Application.Abstractions.Messaging;
using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Domain.Abstraction;

namespace PharmaSaaS.Application.Features.PurchaseOrders.Update;

public record UpdatePurchaseOrderCommand(int Id, UpdatePurchaseOrderRequest Request) : IRequest<Result<PurchaseOrderResponse>>;
