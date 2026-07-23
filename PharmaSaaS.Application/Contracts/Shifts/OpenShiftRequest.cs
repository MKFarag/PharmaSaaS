namespace PharmaSaaS.Application.Contracts.Shifts;

public record OpenShiftRequest(decimal OpeningBalance);

public class OpenShiftRequestValidator : AbstractValidator<OpenShiftRequest>
{
    public OpenShiftRequestValidator()
    {
        RuleFor(x => x.OpeningBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Opening balance must be zero or greater.");
    }
}
