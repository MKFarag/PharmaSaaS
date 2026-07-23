namespace PharmaSaaS.Application.Contracts.Shifts;

public record ShiftResponse(
    int Id,
    DateOnly Date,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    ShiftStatus Status,
    decimal CurrentBalance,
    decimal? Discrepancy
);
