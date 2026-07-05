namespace PharmaSaaS.Domain.Errors;

public record ProductErrors
{
    public static readonly Error NotFound =
        new("Product.NotFound", "No product found", StatusCodes.NotFound);

    public static readonly Error DuplicatedBarcode =
        new("Product.DuplicatedBarcode", "Another product with the same barcode already exists", StatusCodes.Conflict);

    public static readonly Error CompositeDuplicate =
        new("Product.CompositeDuplicate", "A product with the same name, concentration and dosage form already exists", StatusCodes.Conflict);
    
    public static readonly Error CategoryNotFound =
        new("Product.CategoryNotFound", "The specified category does not exist", StatusCodes.NotFound);
    
    public static readonly Error DosageFormNotFound =
        new("Product.DosageFormNotFound", "The specified dosage form does not exist", StatusCodes.NotFound);
    
    public static readonly Error HasInventory =
        new("Product.HasInventory", "Cannot delete product because it has associated inventory records", StatusCodes.Conflict);
}
