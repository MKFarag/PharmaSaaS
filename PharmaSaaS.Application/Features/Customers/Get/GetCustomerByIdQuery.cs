namespace PharmaSaaS.Application.Features.Customers.Get;

public record GetCustomerByIdQuery(int Id) : IRequest<Result<CustomerResponse>>;
