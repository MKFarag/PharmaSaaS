namespace PharmaSaaS.Application.Features.Products.Add;

public record AddProductCommand(ProductRequest Request) : IRequest<Result<ProductResponse>>;
