namespace PharmaSaaS.Application.Contracts.ExpiryReports;

public record ExpiryReportRequestFilters
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchValue { get; init; }
    public int? Year { get; init; }
}

#region Validation

public class ExpiryReportRequestFiltersValidator : AbstractValidator<ExpiryReportRequestFilters>
{
    public ExpiryReportRequestFiltersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 250);

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100)
            .When(x => x.Year.HasValue);
    }
}

#endregion
