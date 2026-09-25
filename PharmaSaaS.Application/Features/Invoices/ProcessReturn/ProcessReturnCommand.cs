namespace PharmaSaaS.Application.Features.Invoices.ProcessReturn;

public record ProcessReturnCommand(int InvoiceId, ProcessReturnRequest Request) : IRequest<Result<ProcessReturnResponse>>;
