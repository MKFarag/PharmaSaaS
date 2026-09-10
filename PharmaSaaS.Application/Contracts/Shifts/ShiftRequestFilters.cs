namespace PharmaSaaS.Application.Contracts.Shifts;

public record ShiftRequestFilters
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public DateOnly? Date { get; init; }
}

#region Validation

public class ShiftRequestFiltersValidator : AbstractValidator<ShiftRequestFilters>
{
    public ShiftRequestFiltersValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 250);
    }
}

#endregion
