namespace PharmaSaaS.Application.Features.Suppliers.Add;

public record AddSupplierCommand(SupplierRequest Request) : IRequest<Result<SupplierResponse>>;
