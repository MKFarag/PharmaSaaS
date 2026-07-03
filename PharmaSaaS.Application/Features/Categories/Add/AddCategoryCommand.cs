namespace PharmaSaaS.Application.Features.Categories.Add;

public record AddCategoryCommand(CategoryRequest Request) : IRequest<Result<CategoryResponse>>;
