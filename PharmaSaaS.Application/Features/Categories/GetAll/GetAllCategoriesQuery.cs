namespace PharmaSaaS.Application.Features.Categories.GetAll;

public record GetAllCategoriesQuery : IRequest<IEnumerable<CategoryResponse>>;
