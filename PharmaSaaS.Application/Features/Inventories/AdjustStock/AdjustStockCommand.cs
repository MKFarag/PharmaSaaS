namespace PharmaSaaS.Application.Features.Inventories.AdjustStock;

public record AdjustStockCommand(int ProductId, AdjustStockRequest Request) : IRequest<Result>;
