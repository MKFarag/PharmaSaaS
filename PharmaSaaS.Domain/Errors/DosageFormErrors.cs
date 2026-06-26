namespace PharmaSaaS.Domain.Errors;

public record DosageFormErrors
{
    public static readonly Error NotFound =
        new("DosageForm.NotFound", "No dosage form found", StatusCodes.NotFound);
    public static readonly Error DuplicatedName =
        new("DosageForm.DuplicatedName", "Another dosage form with the same name is already exists", StatusCodes.Conflict);
}
