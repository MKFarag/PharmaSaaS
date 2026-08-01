namespace PharmaSaaS.Domain.Entities;

public sealed class Expense
{
    public int Id { get; set; }
    public decimal Amount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset Date { get; private set; }

    public int ShiftId { get; private set; }
    public Shift Shift { get; set; } = default!;

    public static Expense Create(int shiftId, decimal amount, string description)
    {
        return new Expense
        {
            ShiftId = shiftId,
            Amount = amount,
            Description = description,
            Date = DateTimeOffset.UtcNow
        };
    }
}
