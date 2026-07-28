using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Domain.Abstraction;

namespace PharmaSaaS.Application.Features.Invoices.GetReturnById;

public record GetReturnInvoiceByIdQuery(int Id) : IRequest<Result<ReturnInvoiceDetailResponse>>;
