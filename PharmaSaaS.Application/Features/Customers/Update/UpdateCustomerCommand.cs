namespace PharmaSaaS.Application.Features.Customers.Update;

public record UpdateCustomerCommand(int Id, CustomerRequest Request) : IRequest<Result>;
