using Mapster;
using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;

namespace PharmaSaaS.Application.Features.Invoices.GetReturnById;

public sealed class GetReturnInvoiceByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetReturnInvoiceByIdQuery, Result<ReturnInvoiceDetailResponse>>
{
    public async Task<Result<ReturnInvoiceDetailResponse>> Handle(GetReturnInvoiceByIdQuery query, CancellationToken cancellationToken)
    {
        if (await unitOfWork.ReturnInvoices.FindAsync(r => r.Id == query.Id, [nameof(ReturnInvoice.Items)], cancellationToken) is not { } returnInvoice)
            return Result.Failure<ReturnInvoiceDetailResponse>(InvoiceErrors.NotFound);

        return Result.Success(returnInvoice.Adapt<ReturnInvoiceDetailResponse>());
    }
}
