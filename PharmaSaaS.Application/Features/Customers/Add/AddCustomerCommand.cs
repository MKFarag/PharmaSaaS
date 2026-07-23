namespace PharmaSaaS.Application.Features.Customers.Add;

public record AddCustomerCommand(CustomerRequest Request) : IRequest<Result<CustomerResponse>>;
