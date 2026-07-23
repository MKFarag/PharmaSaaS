namespace PharmaSaaS.Application.Features.Customers.GetAll;

public record GetAllCustomersQuery(RequestFilters Filters) : IRequest<IPaginatedList<CustomerResponse>>;
