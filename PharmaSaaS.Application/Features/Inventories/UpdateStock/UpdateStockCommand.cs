namespace PharmaSaaS.Application.Features.Inventories.UpdateStock;

public record UpdateStockCommand(int ProductId, UpdateStockRequest Request) : IRequest<Result>;
