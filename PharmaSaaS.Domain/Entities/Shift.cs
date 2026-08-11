namespace PharmaSaaS.Domain.Entities;

public sealed class Shift
{
    public int Id { get; set; }
    public DateOnly Date { get; private set; }
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset? EndTime { get; private set; }

    public string OpenedById { get; private set; } = default!;

    public decimal OpeningBalance { get; private set; }
    public ShiftStatus Status { get; private set; }

    public decimal? ActualBalance { get; private set; }
    public decimal? ExpectedBalance { get; private set; }
    public decimal? Discrepancy { get; private set; }

    public decimal TotalSales { get; private set; }
    public decimal TotalReturns { get; private set; }
    public decimal TotalExpenses { get; private set; }

    public decimal CurrentBalance => OpeningBalance + TotalSales - TotalReturns - TotalExpenses;

    public static Shift Create(string openedById, decimal openingBalance)
    {
        var now = DateTimeOffset.UtcNow;
        return new Shift
        {
            Date = DateOnly.FromDateTime(now.DateTime),
            StartTime = now,
            OpenedById = openedById,
            OpeningBalance = openingBalance,
            Status = ShiftStatus.Open,
            TotalSales = 0,
            TotalReturns = 0,
            TotalExpenses = 0
        };
    }

    public void Close(decimal actualBalance)
    {
        var expectedBalance = OpeningBalance + TotalSales - TotalReturns - TotalExpenses;

        Status = ShiftStatus.Closed;
        EndTime = DateTimeOffset.UtcNow;
        ActualBalance = actualBalance;
        ExpectedBalance = expectedBalance;
        Discrepancy = actualBalance - expectedBalance;
    }

    public void AddSale(decimal amount) => TotalSales += amount;
    public void AddReturn(decimal amount) => TotalReturns += amount;
    public void AddExpense(decimal amount) => TotalExpenses += amount;
    public void RemoveExpense(decimal amount) => TotalExpenses -= amount;
}

