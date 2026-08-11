using PharmaSaaS.Application.Contracts.SupplierPayments;

namespace PharmaSaaS.Application.Features.SupplierPayments.Record;

public record RecordSupplierPaymentCommand(SupplierPaymentRequest Request) : IRequest<Result<SupplierPaymentResponse>>;
