using System.Linq.Expressions;
using PharmaSaaS.Application.Contracts.Common;
using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Domain.Entities;

namespace PharmaSaaS.Application.Features.Invoices.GetAllReturns;

public sealed class GetAllReturnInvoicesQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllReturnInvoicesQuery, Result<IPaginatedList<ReturnInvoiceResponse>>>
{
    public async Task<Result<IPaginatedList<ReturnInvoiceResponse>>> Handle(GetAllReturnInvoicesQuery query, CancellationToken cancellationToken)
    {
        Expression<Func<ReturnInvoice, bool>> predicate = i =>
            (!query.Filters.ShiftId.HasValue || i.ShiftId == query.Filters.ShiftId) &&
            (!query.Filters.InvoiceId.HasValue || i.OriginalInvoiceId == query.Filters.InvoiceId) &&
            (!query.Filters.Date.HasValue || i.CreatedAt.Date == query.Filters.Date.Value.ToDateTime(TimeOnly.MinValue).Date) &&
            (!query.Filters.CustomerId.HasValue || i.OriginalInvoice.CustomerId == query.Filters.CustomerId);

        var returns = await unitOfWork.ReturnInvoices.FindPaginatedListAsync<ReturnInvoiceResponse>(
            predicate,
            query.Filters.PageNumber,
            query.Filters.PageSize,
            nameof(ReturnInvoice.Id),
            [nameof(ReturnInvoice.OriginalInvoice)],
            cancellationToken);

        return Result.Success(returns);
    }
}
