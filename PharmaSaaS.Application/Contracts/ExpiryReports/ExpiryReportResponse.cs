namespace PharmaSaaS.Application.Contracts.ExpiryReports;

public record ExpiryReportResponse(
    int Id,
    float ExpiredQuantity,
    DateOnly ReportDate,
    string? Notes,
    string ReportedByEmployeeId,
    ProductResponse Product
);
