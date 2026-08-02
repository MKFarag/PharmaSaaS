namespace PharmaSaaS.Application.Features.Suppliers.Delete;

public record DeleteSupplierCommand(int Id) : IRequest<Result>;
