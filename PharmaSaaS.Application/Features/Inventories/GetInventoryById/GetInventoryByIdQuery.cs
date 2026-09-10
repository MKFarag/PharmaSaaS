namespace PharmaSaaS.Application.Features.Inventories.GetInventoryById;

public record GetInventoryByIdQuery(int ProductId) : IRequest<Result<InventoryDetailResponse>>;
