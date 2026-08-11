using PharmaSaaS.Application.Contracts.PurchaseOrders;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Application.Abstractions.Messaging;

namespace PharmaSaaS.Application.Features.PurchaseOrders.Approve;

public record ApprovePurchaseOrderCommand(int Id, ApprovePurchaseOrderRequest Request) : IRequest<Result>;
