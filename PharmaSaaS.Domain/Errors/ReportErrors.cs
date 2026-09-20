namespace PharmaSaaS.Domain.Errors;

public record ReportErrors
{
    public static readonly Error InvalidDateRange =
        new("Report.InvalidDateRange", "The 'from' date must be before or equal to the 'to' date", StatusCodes.BadRequest);

    public static readonly Error InvalidLimit =
        new("Report.InvalidLimit", "Limit must be at least 1", StatusCodes.BadRequest);
}
