using PharmaSaaS.Application.Contracts.Inventories;
using PharmaSaaS.Domain.Abstraction;

namespace PharmaSaaS.Application.Features.Inventories.AdjustStock;

public record AdjustStockCommand(int ProductId, AdjustStockRequest Request) : IRequest<Result>;
