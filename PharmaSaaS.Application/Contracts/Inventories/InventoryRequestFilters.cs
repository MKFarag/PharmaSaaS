namespace PharmaSaaS.Application.Contracts.Inventories;

public record InventoryRequestFilters
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? ProductName { get; init; }
    public bool? IsRare { get; init; }
    public bool? BelowTarget { get; init; }
    public bool? OutOfStock { get; init; }
    public bool? NotReordered { get; init; }
    public DateOnly? SoldSince { get; init; }
    public DateOnly? LastOrderedBefore { get; init; }
    public bool? SortByMostSold { get; init; }
}

#region Validation

public class InventoryRequestFiltersValidator : AbstractValidator<InventoryRequestFilters>
{
    public InventoryRequestFiltersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 250);
    }
}

#endregion
