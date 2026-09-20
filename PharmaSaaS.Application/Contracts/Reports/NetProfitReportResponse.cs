namespace PharmaSaaS.Application.Contracts.Reports;

public record NetProfitReportResponse(
    decimal TotalSales,
    decimal TotalExpenses,
    decimal TotalDiscrepancies,
    decimal NetProfit
);
