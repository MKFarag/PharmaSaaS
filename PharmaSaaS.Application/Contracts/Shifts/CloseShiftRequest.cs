namespace PharmaSaaS.Application.Contracts.Shifts;

public record CloseShiftRequest(decimal ActualBalance);

public class CloseShiftRequestValidator : AbstractValidator<CloseShiftRequest>
{
    public CloseShiftRequestValidator()
    {
        RuleFor(x => x.ActualBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Actual balance must be zero or greater.");
    }
}
