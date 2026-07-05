namespace PharmaSaaS.Application.Features.Products.Delete;

public record DeleteProductCommand(int Id) : IRequest<Result>;
