namespace PharmaSaaS.Application.Features.Invoices.GetAll;

public sealed class GetAllInvoicesQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllInvoicesQuery, IPaginatedList<InvoiceResponse>>
{
    public async Task<IPaginatedList<InvoiceResponse>> Handle(GetAllInvoicesQuery query, CancellationToken cancellationToken)
    {
        DateTime? filterDate = query.Filters.Date.HasValue
            ? query.Filters.Date.Value.ToDateTime(TimeOnly.MinValue)
            : null;

        Expression<Func<Invoice, bool>> predicate = i =>
            (!query.Filters.ShiftId.HasValue || i.ShiftId == query.Filters.ShiftId) &&
            (!query.Filters.CustomerId.HasValue || i.CustomerId == query.Filters.CustomerId) &&
            (!filterDate.HasValue || i.CreatedAt.Date == filterDate.Value.Date) &&
            (!query.Filters.FinalAmount.HasValue || i.FinalAmount == query.Filters.FinalAmount.Value);

        var invoices = await unitOfWork.Invoices.FindPaginatedListAsync<InvoiceResponse>(
            predicate,
            query.Filters.PageNumber,
            query.Filters.PageSize,
            nameof(Invoice.Id),
            [nameof(Invoice.Customer)],
            cancellationToken);

        return invoices;
    }
}
