namespace PharmaSaaS.Domain.Errors;

public record PurchaseOrderErrors
{
    public static readonly Error NotFound =
        new("PurchaseOrder.NotFound", "No purchase order found.", StatusCodes.NotFound);

    public static readonly Error AlreadyReceived =
        new("PurchaseOrder.AlreadyReceived", "Purchase order is already received.", StatusCodes.Conflict);

    public static readonly Error InvalidPaidAmount =
        new("PurchaseOrder.InvalidPaidAmount", "Paid amount cannot exceed the total amount.", StatusCodes.BadRequest);

    public static readonly Error ReceivedQuantityExceedsOrdered =
        new("PurchaseOrder.ReceivedQuantityExceedsOrdered", "Received quantity cannot exceed the ordered quantity.", StatusCodes.BadRequest);

    public static readonly Error ItemNotFound =
        new("PurchaseOrder.ItemNotFound", "One or more purchase order items were not found.", StatusCodes.NotFound);

    public static readonly Error InvalidStatus =
        new("PurchaseOrder.InvalidStatus", "Invalid purchase order status for this operation.", StatusCodes.BadRequest);

    public static readonly Error InvalidReturnQuantity =
        new("PurchaseOrder.InvalidReturnQuantity", "Return quantity cannot exceed the received quantity.", StatusCodes.BadRequest);
}
