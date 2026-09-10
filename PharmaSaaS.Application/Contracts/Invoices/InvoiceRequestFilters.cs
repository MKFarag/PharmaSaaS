namespace PharmaSaaS.Application.Contracts.Invoices;

public record InvoiceRequestFilters
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int? ShiftId { get; init; }
    public int? CustomerId { get; init; }
    public DateOnly? Date { get; init; }
    public decimal? FinalAmount { get; init; }
}

#region Validation

public class InvoiceRequestFiltersValidator : AbstractValidator<InvoiceRequestFilters>
{
    public InvoiceRequestFiltersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 250);
    }
}

#endregion
