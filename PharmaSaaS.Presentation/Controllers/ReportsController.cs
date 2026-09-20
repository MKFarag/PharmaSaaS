using PharmaSaaS.Application.Features.Reports.GetDailyReport;
using PharmaSaaS.Application.Features.Reports.GetExpenseReport;
using PharmaSaaS.Application.Features.Reports.GetNetProfitReport;
using PharmaSaaS.Application.Features.Reports.GetShortageReport;
using PharmaSaaS.Application.Features.Reports.GetTopProductsReport;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Business intelligence and analytics reports.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Sliding)]
public class ReportsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Returns the daily summary report for a given date.
    /// </summary>
    /// <remarks>
    /// Aggregates all shifts for the specified date (defaults to today) and returns total sales,
    /// returns, expenses, cash discrepancy, net revenue, and the top 5 selling products.
    ///
    /// Sample request:
    ///
    ///     GET /api/Reports/daily?date=2024-01-15
    ///
    /// </remarks>
    /// <param name="date">The date to report on. Defaults to today (UTC) if omitted.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The daily summary report.</returns>
    /// <response code="200">Returns the daily summary report.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="403">If the user does not have the required permission.</response>
    [HttpGet("daily")]
    [HasPermission(Permissions.ReadDailyReport)]
    [ProducesResponseType(typeof(DailyReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDailyReport([FromQuery] DateOnly? date, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetDailyReportQuery(date ?? DateOnly.FromDateTime(DateTime.UtcNow)), cancellationToken));

    /// <summary>
    /// Returns the net profit report for a date range.
    /// </summary>
    /// <remarks>
    /// Calculates: Net Profit = Total Sales - Total Expenses - Cash Discrepancies.
    /// Cash discrepancies are the absolute sum of negative shift discrepancies (cash shortfalls).
    /// Purchase order costs are funded by owner capital and are not included in this calculation.
    ///
    /// Sample request:
    ///
    ///     GET /api/Reports/profit?from=2024-01-01&amp;to=2024-01-31
    ///
    /// </remarks>
    /// <param name="from">Start date (inclusive).</param>
    /// <param name="to">End date (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The net profit breakdown.</returns>
    /// <response code="200">Returns the net profit report.</response>
    /// <response code="400">If 'from' is after 'to'.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="403">If the user does not have the required permission.</response>
    [HttpGet("profit")]
    [HasPermission(Permissions.ReadProfitReport)]
    [ProducesResponseType(typeof(NetProfitReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetNetProfit([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNetProfitReportQuery(from, to), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Returns all products currently in shortage.
    /// </summary>
    /// <remarks>
    /// A product is in shortage if it is manually flagged as rare (IsRare = true) or its
    /// quantity has dropped to 25% or below its configured target stock (Auto shortage).
    /// The response includes a ShortageType field: "Manual", "Auto", or "Both".
    ///
    /// Sample request:
    ///
    ///     GET /api/Reports/shortages
    ///
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of shortage inventory items.</returns>
    /// <response code="200">Returns the shortage report.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="403">If the user does not have the required permission.</response>
    [HttpGet("shortages")]
    [HasPermission(Permissions.ReadShortageReport)]
    [ProducesResponseType(typeof(IEnumerable<ShortageReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetShortageReport(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetShortageReportQuery(), cancellationToken));

    /// <summary>
    /// Returns all expenses logged within a date range.
    /// </summary>
    /// <remarks>
    /// Each expense entry includes the parent shift's date for easy grouping on the client side.
    ///
    /// Sample request:
    ///
    ///     GET /api/Reports/expenses?from=2024-01-01&amp;to=2024-01-31
    ///
    /// </remarks>
    /// <param name="from">Start date (inclusive).</param>
    /// <param name="to">End date (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of expense entries.</returns>
    /// <response code="200">Returns the expense report.</response>
    /// <response code="400">If 'from' is after 'to'.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="403">If the user does not have the required permission.</response>
    [HttpGet("expenses")]
    [HasPermission(Permissions.ReadExpenseReport)]
    [ProducesResponseType(typeof(IEnumerable<ExpenseReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetExpenseReport([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetExpenseReportQuery(from, to), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Returns the top-selling products by quantity sold within a date range.
    /// </summary>
    /// <remarks>
    /// Products are ranked by total units sold descending. Revenue is calculated
    /// from invoice line items (quantity × unit price at time of sale).
    ///
    /// Sample request:
    ///
    ///     GET /api/Reports/top-products?from=2024-01-01&amp;to=2024-01-31&amp;limit=10
    ///
    /// </remarks>
    /// <param name="from">Start date (inclusive).</param>
    /// <param name="to">End date (inclusive).</param>
    /// <param name="limit">Maximum number of products to return. Defaults to 10.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A ranked list of top-selling products.</returns>
    /// <response code="200">Returns the top-selling products.</response>
    /// <response code="400">If 'from' is after 'to', or limit is less than 1.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="403">If the user does not have the required permission.</response>
    [HttpGet("top-products")]
    [HasPermission(Permissions.ReadTopProductsReport)]
    [ProducesResponseType(typeof(IEnumerable<TopProductReportResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTopProducts([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetTopProductsReportQuery(from, to, limit), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
