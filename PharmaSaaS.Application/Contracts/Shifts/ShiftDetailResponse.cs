namespace PharmaSaaS.Application.Contracts.Shifts;

public record ShiftDetailResponse(
    int Id,
    DateOnly Date,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    string OpenedById,
    ShiftStatus Status,
    decimal OpeningBalance,
    decimal TotalSales,
    decimal TotalReturns,
    decimal TotalExpenses,
    decimal? ExpectedBalance,
    decimal? ActualBalance,
    decimal? Discrepancy
);
