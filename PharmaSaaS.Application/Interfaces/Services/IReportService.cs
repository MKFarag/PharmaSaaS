namespace PharmaSaaS.Application.Interfaces.Services;

public interface IReportService
{
    Task<DailyReportResponse> GetDailyReportAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<NetProfitReportResponse> GetNetProfitReportAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<IEnumerable<TopProductReportResponse>> GetTopSellingProductsAsync(DateOnly from, DateOnly to, int limit, CancellationToken cancellationToken = default);
}
