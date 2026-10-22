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
            .Matches(RegexPatterns.BusinessName)
            .WithMessage("Supplier name must start and end with a letter or number, and can contain spaces, hyphens, apostrophes, periods, underscores, and ampersands.")
            .MaximumLength(200);

        RuleFor(x => x.ContactInfo)
            .MaximumLength(500);
    }
}

#endregion
