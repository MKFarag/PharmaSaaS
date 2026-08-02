namespace PharmaSaaS.Application.Features.Suppliers.GetAll;

public record GetAllSuppliersQuery() : IRequest<IEnumerable<SupplierResponse>>;
