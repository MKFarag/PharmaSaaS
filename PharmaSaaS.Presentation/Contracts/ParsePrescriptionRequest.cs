namespace PharmaSaaS.Presentation.Contracts;

public record ParsePrescriptionRequest(
    IFormFile Image
);

public class ParsePrescriptionRequestValidator : AbstractValidator<ParsePrescriptionRequest>
{
    public ParsePrescriptionRequestValidator()
    {
        RuleFor(x => x.Image)
            .SetValidator(new PrescriptionImageValidator());
    }
}
