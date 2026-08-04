namespace PharmaSaaS.Domain.Errors;

public record PurchaseOrderErrors
{
    public static readonly Error NotFound =
        new("PurchaseOrder.NotFound", "No purchase order found.", StatusCodes.NotFound);

    public static readonly Error AlreadyReceived =
        new("PurchaseOrder.AlreadyReceived", "Purchase order is already received.", StatusCodes.Conflict);
}
