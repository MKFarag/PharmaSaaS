namespace PharmaSaaS.Application.Features.Invoices.Create;

public record CreateInvoiceCommand(CreateInvoiceRequest Request) : IRequest<Result<InvoiceDetailResponse>>;
