namespace PharmaSaaS.Application.Contracts.Products;

public record ProductRequestFilters
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchValue { get; init; }
    public int? CategoryId { get; init; }
    public int? DosageFormId { get; init; }
    public bool? OutOfStock { get; init; }
    public bool? BelowTarget { get; init; }
    public bool? NotReordered { get; init; }
    public DateOnly? SoldSince { get; init; }
    public DateOnly? LastOrderedBefore { get; init; }
    public bool? SortByMostSold { get; init; }
}

#region Validation

public class ProductRequestFiltersValidator : AbstractValidator<ProductRequestFilters>
{
    public ProductRequestFiltersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 250);
    }
}

#endregion
