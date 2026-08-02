namespace PharmaSaaS.Application.Features.Suppliers.ResetBalance;

public record ResetSupplierBalanceCommand(int Id) : IRequest<Result>;
