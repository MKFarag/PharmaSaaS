using PharmaSaaS.Application.Contracts.Invoices;

namespace PharmaSaaS.Application.Features.Invoices.ProcessReturn;

public record ProcessReturnCommand(int InvoiceId, ProcessReturnRequest Request, string UserId) : IRequest<Result<ProcessReturnResponse>>;
