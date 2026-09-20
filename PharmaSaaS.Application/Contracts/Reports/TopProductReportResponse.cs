namespace PharmaSaaS.Application.Contracts.Reports;

public record TopProductReportResponse(
    int ProductId,
    string ProductName,
    float TotalQuantitySold,
    decimal TotalRevenue
);
