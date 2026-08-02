namespace PharmaSaaS.Domain.Errors;

public record SupplierErrors
{
    public static readonly Error NotFound =
        new("Supplier.NotFound", "No supplier found.", StatusCodes.NotFound);

    public static readonly Error DuplicateName =
        new("Supplier.DuplicateName", "A supplier with the same name already exists.", StatusCodes.Conflict);

    public static readonly Error HasOrders =
        new("Supplier.HasOrders", "Cannot delete a supplier that has linked purchase orders.", StatusCodes.Conflict);
}
