using PharmaSaaS.Application.Contracts.SupplierPayments;

namespace PharmaSaaS.Application.Features.SupplierPayments.List;

public record ListSupplierPaymentsQuery(SupplierPaymentRequestFilters Filters) : IRequest<IPaginatedList<SupplierPaymentResponse>>;
