namespace PharmaSaaS.Application.Contracts.DosageForms;

public record DosageFormRequest(
    string Name
);

public class DosageFormRequestValidator : AbstractValidator<DosageFormRequest>
{
    public DosageFormRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(2, 100);
    }
}
