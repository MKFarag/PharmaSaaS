namespace PharmaSaaS.Domain.Errors;

public record PharmacyErrors
{
    public static readonly Error NotFound =
        new("Pharmacy.NotFound", "No pharmacy found", StatusCodes.NotFound);

    public static readonly Error DuplicatedName =
        new("Pharmacy.DuplicatedName", "Another pharmacy with the same name is already exists under this tenant", StatusCodes.Conflict);

    public static readonly Error CreationCanceled =
        new("Pharmacy.CreationCanceled", "Pharmacy creation was canceled", StatusCodes.RequestCanceled);
}
