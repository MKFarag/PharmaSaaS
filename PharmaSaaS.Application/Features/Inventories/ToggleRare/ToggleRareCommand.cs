namespace PharmaSaaS.Application.Features.Inventories.ToggleRare;

public record ToggleRareCommand(int ProductId) : IRequest<Result>;
