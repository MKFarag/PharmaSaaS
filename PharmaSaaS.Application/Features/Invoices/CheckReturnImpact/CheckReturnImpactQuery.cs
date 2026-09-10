namespace PharmaSaaS.Application.Features.Invoices.CheckReturnImpact;

public record CheckReturnImpactQuery(int InvoiceId, ProcessReturnRequest Request) : IRequest<Result<CheckReturnImpactResponse>>;
