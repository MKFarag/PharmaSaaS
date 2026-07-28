using PharmaSaaS.Application.Contracts.Invoices;

namespace PharmaSaaS.Application.Features.Invoices.GetById;

public record GetInvoiceByIdQuery(int Id) : IRequest<Result<InvoiceDetailResponse>>;
