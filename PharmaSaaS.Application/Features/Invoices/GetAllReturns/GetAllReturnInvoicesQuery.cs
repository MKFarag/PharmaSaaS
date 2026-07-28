using PharmaSaaS.Application.Contracts.Common;
using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Domain.Abstraction;

namespace PharmaSaaS.Application.Features.Invoices.GetAllReturns;

public record GetAllReturnInvoicesQuery(ReturnInvoiceRequestFilters Filters) : IRequest<Result<IPaginatedList<ReturnInvoiceResponse>>>;
