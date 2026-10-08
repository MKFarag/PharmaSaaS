namespace PharmaSaaS.Application.Contracts.AI;

public record SuggestMedicationsRequest(string Description);

#region Validation

public class SuggestMedicationsRequestValidator : AbstractValidator<SuggestMedicationsRequest>
{
    public SuggestMedicationsRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);
    }
}

#endregion
