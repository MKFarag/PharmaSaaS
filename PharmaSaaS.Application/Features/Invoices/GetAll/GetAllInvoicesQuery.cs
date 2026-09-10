namespace PharmaSaaS.Application.Features.Invoices.GetAll;

public record GetAllInvoicesQuery(InvoiceRequestFilters Filters) : IRequest<IPaginatedList<InvoiceResponse>>;
