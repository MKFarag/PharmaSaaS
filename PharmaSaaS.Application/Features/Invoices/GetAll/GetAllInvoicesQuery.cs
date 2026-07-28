using PharmaSaaS.Application.Contracts.Invoices;

namespace PharmaSaaS.Application.Features.Invoices.GetAll;

public record GetAllInvoicesQuery(InvoiceRequestFilters Filters) : IRequest<Result<IPaginatedList<InvoiceResponse>>>;
