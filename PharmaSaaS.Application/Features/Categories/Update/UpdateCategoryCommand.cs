namespace PharmaSaaS.Application.Features.Categories.Update;

public record UpdateCategoryCommand(int Id, CategoryRequest Request) : IRequest<Result>;
