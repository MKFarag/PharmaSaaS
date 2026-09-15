namespace PharmaSaaS.Application.Contracts.Products;

public record ProductRequest(
    string Name,
    string? Barcode,
    string? Concentration,
    decimal? MaxStorageTemperature,
    int CategoryId,
    int DosageFormId,
    int? StripsPerBox
);

#region Validation

public class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(2, 200);

        RuleFor(x => x.Barcode)
            .MaximumLength(50)
            .When(x => x.Barcode is not null);

        RuleFor(x => x.Concentration)
            .MaximumLength(50)
            .When(x => x.Concentration is not null);

        RuleFor(x => x.MaxStorageTemperature)
            .GreaterThan(0)
            .When(x => x.MaxStorageTemperature is not null);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0);

        RuleFor(x => x.DosageFormId)
            .GreaterThan(0);

        RuleFor(x => x.StripsPerBox)
            .GreaterThanOrEqualTo(2)
            .When(x => x.StripsPerBox is not null);
    }
}

#endregion
