namespace PharmaSaaS.Domain.Errors;

public record SupplierPaymentErrors
{
    public static readonly Error NotFound =
        new("SupplierPayment.NotFound", "No supplier payment found.", StatusCodes.NotFound);

    public static readonly Error InvalidAmount =
        new("SupplierPayment.InvalidAmount", "Payment amount must be greater than zero.", StatusCodes.BadRequest);

    public static readonly Error OrderNotBelongToSupplier =
        new("SupplierPayment.OrderNotBelongToSupplier", "One or more orders do not belong to this supplier.", StatusCodes.BadRequest);
}
