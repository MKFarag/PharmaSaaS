namespace PharmaSaaS.Application.Contracts.PurchaseOrders;

public record PurchaseOrderRequestFilters
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int? SupplierId { get; init; }
    public DateOnly? Date { get; init; }
}

#region Validation

public class PurchaseOrderRequestFiltersValidator : AbstractValidator<PurchaseOrderRequestFilters>
{
    public PurchaseOrderRequestFiltersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 250);
    }
}

#endregion
