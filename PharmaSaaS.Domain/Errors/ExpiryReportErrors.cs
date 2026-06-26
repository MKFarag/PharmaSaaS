namespace PharmaSaaS.Domain.Errors;

public record ExpiryReportErrors
{
    public static readonly Error NotFound =
        new("ExpiryReport.NotFound", "No expiry report found", StatusCodes.NotFound);
    public static readonly Error ProductNotFound =
        new("ExpiryReport.ProductNotFound", "The specified product does not exist", StatusCodes.NotFound);
}
