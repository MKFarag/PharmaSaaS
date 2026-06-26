namespace PharmaSaaS.Domain.Errors;

public record ProductErrors
{
    public static readonly Error NotFound =
        new("Product.NotFound", "No product found", StatusCodes.NotFound);
    public static readonly Error DuplicatedBarcode =
        new("Product.DuplicatedBarcode", "Another product with the same barcode is already exists", StatusCodes.Conflict);
    public static readonly Error CategoryNotFound =
        new("Product.CategoryNotFound", "The specified category does not exist", StatusCodes.NotFound);
    public static readonly Error DosageFormNotFound =
        new("Product.DosageFormNotFound", "The specified dosage form does not exist", StatusCodes.NotFound);
}
