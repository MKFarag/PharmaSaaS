namespace PharmaSaaS.Domain.Errors;

public record InvoiceErrors
{
    public static readonly Error NotFound =
        new("Invoice.NotFound", "No invoice found with the specified ID", StatusCodes.NotFound);

    public static readonly Error ShiftNotFound =
        new("Invoice.ShiftNotFound", "No shift found with the specified ID", StatusCodes.NotFound);

    public static readonly Error ShiftNotOpen =
        new("Invoice.ShiftNotOpen", "The specified shift is not open", StatusCodes.Conflict);

    public static readonly Error ProductNotFound =
        new("Invoice.ProductNotFound", "One or more products were not found in inventory", StatusCodes.NotFound);

    public static readonly Error InsufficientStock =
        new("Invoice.InsufficientStock", "Insufficient stock for one or more products", StatusCodes.Conflict);

    public static readonly Error ProductNotInInvoice =
        new("Invoice.ProductNotInInvoice", "The specified product is not part of this invoice", StatusCodes.BadRequest);

    public static readonly Error ReturnQuantityExceeded =
        new("Invoice.ReturnQuantityExceeded", "The return quantity exceeds the originally purchased quantity", StatusCodes.Conflict);
}
