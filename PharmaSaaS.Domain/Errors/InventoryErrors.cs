namespace PharmaSaaS.Domain.Errors;

public record InventoryErrors
{
    public static readonly Error NotFound =
        new("Inventory.NotFound", "No inventory record found for this product", StatusCodes.NotFound);

    public static readonly Error AlreadyExists =
        new("Inventory.AlreadyExists", "An inventory record already exists for this product", StatusCodes.Conflict);

    public static readonly Error QuantityExceedsStock =
        new("Inventory.QuantityExceedsStock", "Expired quantity exceeds available stock", StatusCodes.BadRequest);

    public static readonly Error ProductNotFound =
        new("Inventory.ProductNotFound", "The specified product was not found", StatusCodes.NotFound);

    public static readonly Error InsufficientStock =
        new("Inventory.InsufficientStock", "Stock adjustment would result in negative quantity", StatusCodes.BadRequest);
}
