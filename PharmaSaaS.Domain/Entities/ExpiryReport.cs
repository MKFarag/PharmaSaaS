namespace PharmaSaaS.Domain.Entities;

public sealed class ExpiryReport
{
    public int Id { get; set; }
    public float ExpiredQuantity { get; private set; }
    public DateOnly ReportDate { get; private set; }
    public string? Notes { get; private set; }
    public int ProductId { get; private set; }
    public string ReportedByEmployeeId { get; private set; } = string.Empty;

    public Product Product { get; set; } = default!;

    public static ExpiryReport Create(int productId, float expiredQuantity, string reportedByEmployeeId, string? notes = null)
        => new()
        {
            ProductId = productId,
            ExpiredQuantity = expiredQuantity,
            ReportDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ReportedByEmployeeId = reportedByEmployeeId,
            Notes = notes
        };

    public void Update(float expiredQuantity, string? notes = null)
    {
        ExpiredQuantity = expiredQuantity;
        Notes = notes;
    }
}
