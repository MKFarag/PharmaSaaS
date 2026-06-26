namespace PharmaSaaS.Domain.Errors;

public record InventoryErrors
{
    public static readonly Error NotFound =
        new("Inventory.NotFound", "No inventory record found for this product", StatusCodes.NotFound);
    public static readonly Error AlreadyExists =
        new("Inventory.AlreadyExists", "An inventory record already exists for this product", StatusCodes.Conflict);
}
