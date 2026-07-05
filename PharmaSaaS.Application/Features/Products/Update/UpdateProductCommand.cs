namespace PharmaSaaS.Application.Features.Products.Update;

public record UpdateProductCommand(int Id, ProductRequest Request) : IRequest<Result>;
