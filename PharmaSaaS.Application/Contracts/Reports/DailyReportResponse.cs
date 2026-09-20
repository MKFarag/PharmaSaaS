namespace PharmaSaaS.Application.Contracts.Reports;

public record DailyReportResponse(
    DateOnly Date,
    decimal TotalSales,
    decimal TotalReturns,
    decimal TotalExpenses,
    decimal? Discrepancy,
    decimal NetRevenue,
    IEnumerable<TopProductReportResponse> TopProducts
);
