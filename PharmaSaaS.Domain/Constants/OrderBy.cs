namespace PharmaSaaS.Domain.Constants;

public static class OrderBy
{
    public const string Ascending = "ASC";
    public const string Descending = "DESC";

    public static bool IsValid(string? orderByType)
        => orderByType is not null &&
            (string.Equals(orderByType, Ascending, StringComparison.OrdinalIgnoreCase)
            || string.Equals(orderByType, Descending, StringComparison.OrdinalIgnoreCase));
}
