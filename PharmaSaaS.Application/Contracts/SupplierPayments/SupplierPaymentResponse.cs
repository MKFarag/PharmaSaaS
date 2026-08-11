namespace PharmaSaaS.Application.Contracts.SupplierPayments;

public record SupplierPaymentResponse(
    int Id,
    int SupplierId,
    string SupplierName,
    decimal Amount,
    DateTime PaidAt,
    string? Notes,
    IList<int> OrderIds
);
