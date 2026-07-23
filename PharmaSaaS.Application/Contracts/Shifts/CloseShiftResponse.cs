namespace PharmaSaaS.Application.Contracts.Shifts;

public record CloseShiftResponse(
    decimal ExpectedBalance,
    decimal Discrepancy
);
