namespace PharmaSaaS.Domain.Entities;

public sealed class AIUsageLog
{
    public int Id { get; set; }
    public string PharmacyId { get; private set; } = string.Empty;
    public DateTime UsedAt { get; private set; }

    public static AIUsageLog Create(string pharmacyId) => new() { PharmacyId = pharmacyId, UsedAt = DateTime.UtcNow };
}
