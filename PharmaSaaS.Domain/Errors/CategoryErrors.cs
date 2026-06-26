namespace PharmaSaaS.Domain.Errors;

public record CategoryErrors
{
    public static readonly Error NotFound =
        new("Category.NotFound", "No category found", StatusCodes.NotFound);
    public static readonly Error DuplicatedName =
        new("Category.DuplicatedName", "Another category with the same name is already exists", StatusCodes.Conflict);
}
