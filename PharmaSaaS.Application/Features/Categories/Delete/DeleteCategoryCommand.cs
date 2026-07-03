namespace PharmaSaaS.Application.Features.Categories.Delete;

public record DeleteCategoryCommand(int Id) : IRequest<Result>;
