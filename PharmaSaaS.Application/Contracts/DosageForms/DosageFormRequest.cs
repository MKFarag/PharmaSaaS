namespace PharmaSaaS.Application.Contracts.DosageForms;

public record DosageFormRequest(
    string Name
);

#region Validation

public class DosageFormRequestValidator : AbstractValidator<DosageFormRequest>
{
    public DosageFormRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Matches(RegexPatterns.BusinessName)
            .WithMessage("Dosage form name must start and end with a letter or number, and can contain spaces, hyphens, apostrophes, periods, underscores, and ampersands.")
            .Length(2, 100);
    }
}

#endregion
