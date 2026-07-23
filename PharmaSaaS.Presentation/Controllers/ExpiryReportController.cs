using PharmaSaaS.Application.Contracts.ExpiryReports;
using PharmaSaaS.Application.Features.ExpiryReports.Create;
using PharmaSaaS.Application.Features.ExpiryReports.GetAll;
using PharmaSaaS.Application.Features.ExpiryReports.Revert;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage expiry reports and related stock deductions.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]
public class ExpiryReportController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of expiry reports.
    /// </summary>
    /// <remarks>
    /// Returns expiry reports, optionally filtered by year and searchable by product name.
    ///
    /// Sample requests:
    ///
    ///     GET /api/ExpiryReport?pageNumber=1&amp;pageSize=10
    ///     GET /api/ExpiryReport?year=2026
    ///     GET /api/ExpiryReport?searchValue=panadol
    ///
    /// </remarks>
    /// <param name="filters">Pagination, search, and year filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of expiry reports.</returns>
    /// <response code="200">Returns the paginated expiry reports list.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("")]
    [HasPermission(Permissions.ReadExpiryReport)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] ExpiryReportRequestFilters filters, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAllExpiryReportsQuery(filters), cancellationToken));

    /// <summary>
    /// Logs an expiry report for a product.
    /// </summary>
    /// <remarks>
    /// Records expired stock for a product and deducts the expired quantity from inventory.
    ///
    /// Sample request:
    ///
    ///     POST /api/ExpiryReport
    ///     {
    ///       "productId": 1,
    ///       "expiredQuantity": 10,
    ///       "notes": "Found expired during monthly check"
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The expiry report request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the expiry report was logged successfully.</response>
    /// <response code="400">If the request is invalid or quantity exceeds stock.</response>
    /// <response code="404">If the product is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpPost("")]
    [HasPermission(Permissions.LogExpiryReport)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogExpiryReport([FromBody] ExpiryReportRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new LogExpiryReportCommand(request, User.GetId()!), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Reverts an expiry report.
    /// </summary>
    /// <remarks>
    /// Deletes the expiry report and returns the expired quantity back to the inventory stock.
    /// </remarks>
    /// <param name="id">The expiry report identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the report was successfully reverted.</response>
    /// <response code="404">If the report is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.RevertExpiryReport)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RevertExpiryReport([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RevertExpiryReportCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}
