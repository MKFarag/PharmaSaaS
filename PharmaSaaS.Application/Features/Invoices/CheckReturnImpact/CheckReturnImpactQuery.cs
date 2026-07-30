using PharmaSaaS.Application.Abstractions.Messaging;
using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Domain.Abstraction;

namespace PharmaSaaS.Application.Features.Invoices.CheckReturnImpact;

/// <summary>
/// Query that previews the financial and loyalty impact of processing a return
/// without actually committing any changes.
/// </summary>
public record CheckReturnImpactQuery(
    int InvoiceId,
    ProcessReturnRequest Request
) : IRequest<Result<CheckReturnImpactResponse>>;
