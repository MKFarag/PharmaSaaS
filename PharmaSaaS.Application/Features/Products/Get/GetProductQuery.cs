namespace PharmaSaaS.Application.Features.Products.Get;

public record GetProductQuery(int Id) : IRequest<Result<ProductResponse>>;
