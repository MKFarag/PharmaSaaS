namespace PharmaSaaS.Application.Features.Suppliers.Update;

public record UpdateSupplierCommand(int Id, SupplierRequest Request) : IRequest<Result>;
