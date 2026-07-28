using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Application.Interfaces.Infrastructure;

namespace PharmaSaaS.Application.Features.Invoices.GetById;

public sealed class GetInvoiceByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDetailResponse>>
{
    public async Task<Result<InvoiceDetailResponse>> Handle(GetInvoiceByIdQuery query, CancellationToken cancellationToken)
    {
        if (await unitOfWork.Invoices.FindAsync(i => i.Id == query.Id, Invoice.DetailIncludes, cancellationToken) is not { } invoice)
            return Result.Failure<InvoiceDetailResponse>(InvoiceErrors.NotFound);

        return Result.Success(invoice.Adapt<InvoiceDetailResponse>());
    }
}
