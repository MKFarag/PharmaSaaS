namespace PharmaSaaS.Application.Contracts.Pharmacies;

public record PharmacyRequest(string Name);

#region Validation

public class PharmacyRequestValidator : AbstractValidator<PharmacyRequest>
{
    public PharmacyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(2, 200);
    }
}

#endregion
