namespace PharmaSaaS.Application.Features.Invoices.Create;

public record CreateInvoiceCommand(CreateInvoiceRequest Request, string UserId) : IRequest<Result<InvoiceDetailResponse>>;
