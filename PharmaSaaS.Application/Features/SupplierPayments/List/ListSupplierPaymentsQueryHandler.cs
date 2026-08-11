using PharmaSaaS.Domain.Entities;

namespace PharmaSaaS.Application.Features.SupplierPayments.List;

public sealed class ListSupplierPaymentsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<ListSupplierPaymentsQuery, IPaginatedList<SupplierPaymentResponse>>
{
    public async Task<IPaginatedList<SupplierPaymentResponse>> Handle(ListSupplierPaymentsQuery query, CancellationToken cancellationToken = default)
    {
        var payments = await unitOfWork.SupplierPayments.FindPaginatedListAsync<SupplierPaymentResponse>(
            sp => !query.Filters.SupplierId.HasValue || sp.SupplierId == query.Filters.SupplierId.Value,
            query.Filters.PageNumber,
            query.Filters.PageSize,
            nameof(SupplierPayment.PaidAt),
            [nameof(SupplierPayment.Supplier), nameof(SupplierPayment.Orders)],
            cancellationToken);

        return payments;
    }
}
