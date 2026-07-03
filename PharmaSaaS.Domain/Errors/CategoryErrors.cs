namespace PharmaSaaS.Domain.Errors;

public record CategoryErrors
{
    public static readonly Error NotFound =
        new("Category.NotFound", "No category found", StatusCodes.NotFound);

    public static readonly Error DuplicatedName =
        new("Category.DuplicatedName", "Another category with the same name is already exists", StatusCodes.Conflict);

    public static readonly Error IsUsed =
        new("Category.IsUsed", "Cannot delete category because it is associated with one or more products", StatusCodes.Conflict);
}
