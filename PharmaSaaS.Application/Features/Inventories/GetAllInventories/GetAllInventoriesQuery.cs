namespace PharmaSaaS.Application.Features.Inventories.GetAllInventories;

public record GetAllInventoriesQuery(InventoryRequestFilters Filters) : IRequest<IPaginatedList<InventoryResponse>>;
