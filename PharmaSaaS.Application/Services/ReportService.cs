namespace PharmaSaaS.Application.Services;

public sealed class ReportService(IUnitOfWork unitOfWork) : IReportService
{
    public async Task<DailyReportResponse> GetDailyReportAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var shifts = await unitOfWork.Shifts.FindAllAsync(s => s.Date == date, cancellationToken);

        decimal totalSales = shifts.Sum(s => s.TotalSales);
        decimal totalReturns = shifts.Sum(s => s.TotalReturns);
        decimal totalExpenses = shifts.Sum(s => s.TotalExpenses);
        decimal? discrepancy = shifts.Any(s => s.Discrepancy.HasValue)
            ? shifts.Sum(s => s.Discrepancy ?? 0)
            : null;
        decimal netRevenue = totalSales - totalReturns - totalExpenses;

        var startDate = date.ToDateTime(TimeOnly.MinValue);
        var endDate = date.ToDateTime(TimeOnly.MaxValue);

        var invoices = await unitOfWork.Invoices.FindAllAsync(
            i => i.CreatedAt >= startDate && i.CreatedAt <= endDate,
            [nameof(Invoice.Items), $"{nameof(Invoice.Items)}.{nameof(InvoiceItem.Product)}"],
            cancellationToken);

        var topProducts = invoices
            .SelectMany(i => i.Items)
            .GroupBy(item => item.ProductId)
            .Select(g => new TopProductReportResponse(
                g.Key,
                g.First().Product.Name,
                g.Sum(i => i.Quantity),
                g.Sum(i => (decimal)i.Quantity * i.UnitPrice)))
            .OrderByDescending(p => p.TotalQuantitySold)
            .Take(5);

        return new DailyReportResponse(date, totalSales, totalReturns, totalExpenses, discrepancy, netRevenue, topProducts);
    }

    public async Task<NetProfitReportResponse> GetNetProfitReportAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.ToDateTime(TimeOnly.MaxValue);

        var invoices = await unitOfWork.Invoices.FindAllAsync(
            i => i.CreatedAt >= fromDateTime && i.CreatedAt <= toDateTime,
            cancellationToken);
        decimal totalSales = invoices.Sum(i => i.FinalAmount);

        var expenses = await unitOfWork.Expenses.FindAllAsync(
            e => e.Date >= fromDateTime && e.Date <= toDateTime,
            cancellationToken);
        decimal totalExpenses = expenses.Sum(e => e.Amount);

        var shifts = await unitOfWork.Shifts.FindAllAsync(
            s => s.Date >= from && s.Date <= to && s.Discrepancy.HasValue && s.Discrepancy < 0,
            cancellationToken);
        decimal totalDiscrepancies = Math.Abs(shifts.Sum(s => s.Discrepancy ?? 0));

        decimal netProfit = totalSales - totalExpenses - totalDiscrepancies;

        return new NetProfitReportResponse(totalSales, totalExpenses, totalDiscrepancies, netProfit);
    }

    public async Task<IEnumerable<TopProductReportResponse>> GetTopSellingProductsAsync(DateOnly from, DateOnly to, int limit, CancellationToken cancellationToken = default)
    {
        var fromDateTime = from.ToDateTime(TimeOnly.MinValue);
        var toDateTime = to.ToDateTime(TimeOnly.MaxValue);

        var invoices = await unitOfWork.Invoices.FindAllAsync(
            i => i.CreatedAt >= fromDateTime && i.CreatedAt <= toDateTime,
            [nameof(Invoice.Items), $"{nameof(Invoice.Items)}.{nameof(InvoiceItem.Product)}"],
            cancellationToken);

        return invoices
            .SelectMany(i => i.Items)
            .GroupBy(item => item.ProductId)
            .Select(g => new TopProductReportResponse(
                g.Key,
                g.First().Product.Name,
                g.Sum(i => i.Quantity),
                g.Sum(i => (decimal)i.Quantity * i.UnitPrice)))
            .OrderByDescending(p => p.TotalQuantitySold)
            .Take(limit);
    }
}
