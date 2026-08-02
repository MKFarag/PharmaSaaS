namespace PharmaSaaS.Application.Features.Suppliers.Get;

public record GetSupplierByIdQuery(int Id) : IRequest<Result<SupplierResponse>>;
