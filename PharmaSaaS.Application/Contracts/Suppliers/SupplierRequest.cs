namespace PharmaSaaS.Application.Contracts.Suppliers;

public record SupplierRequest(
    string Name,
    string? ContactInfo
);

#region Validation

public class SupplierRequestValidator : AbstractValidator<SupplierRequest>
{
    public SupplierRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.ContactInfo)
            .MaximumLength(500);
    }
}

#endregion
