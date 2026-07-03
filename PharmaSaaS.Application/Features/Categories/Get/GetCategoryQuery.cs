namespace PharmaSaaS.Application.Features.Categories.Get;

public record GetCategoryQuery(int Id) : IRequest<Result<CategoryResponse>>;
