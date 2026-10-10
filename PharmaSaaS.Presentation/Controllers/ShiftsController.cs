using PharmaSaaS.Application.Contracts.Shifts;
using PharmaSaaS.Application.Features.Shifts.Close;
using PharmaSaaS.Application.Features.Shifts.GetAll;
using PharmaSaaS.Application.Features.Shifts.GetById;
using PharmaSaaS.Application.Features.Shifts.Open;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage daily shifts and cash reconciliation.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]
public class ShiftsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves all shifts.
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of shifts, optionally filtered by date.
    ///
    /// Sample requests:
    ///
    ///     GET /api/shifts?pageNumber=1&amp;pageSize=10
    ///     GET /api/shifts?date=2026-07-23
    ///
    /// </remarks>
    /// <param name="filters">Pagination, search, and sort filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of shifts.</returns>
    /// <response code="200">Returns the paginated shifts list.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("")]
    [HasPermission(Permissions.ReadShift)]
    [ProducesResponseType(typeof(IPaginatedList<ShiftResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] ShiftRequestFilters filters, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAllShiftsQuery(filters), cancellationToken));

    /// <summary>
    /// Retrieves a specific shift by ID.
    /// </summary>
    /// <remarks>
    /// Returns full shift details including sales, returns, expenses, and reconciliation data.
    ///
    /// Sample request:
    ///
    ///     GET /api/shifts/1
    ///
    /// </remarks>
    /// <param name="id">The unique identifier of the shift.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The shift details.</returns>
    /// <response code="200">Returns the shift details.</response>
    /// <response code="404">If the shift is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.ReadShift)]
    [ProducesResponseType(typeof(ShiftDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetShiftByIdQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Opens the daily shift.
    /// </summary>
    /// <remarks>
    /// Creates a new shift for today. Only one open shift is allowed per pharmacy per day.
    ///
    /// Sample request:
    ///
    ///     POST /api/shifts/open
    ///     {
    ///       "openingBalance": 500.00
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The opening balance for the shift.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created shift.</returns>
    /// <response code="201">Returns the newly created shift.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If an open shift already exists for today.</response>
    [HttpPost("open")]
    [HasPermission(Permissions.OpenShift)]
    [ProducesResponseType(typeof(ShiftResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Open([FromBody] OpenShiftRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new OpenShiftCommand(request, User.GetId()!), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Closes an open shift.
    /// </summary>
    /// <remarks>
    /// Closes the specified shift, calculates the expected balance, and returns the cash discrepancy.
    ///
    /// Sample request:
    ///
    ///     POST /api/shifts/1/close
    ///     {
    ///       "actualBalance": 1250.00
    ///     }
    ///
    /// </remarks>
    /// <param name="id">The unique identifier of the shift to close.</param>
    /// <param name="request">The actual cash balance counted at end of shift.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The expected balance and discrepancy.</returns>
    /// <response code="200">Returns the reconciliation result.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the shift is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If the shift is already closed.</response>
    [HttpPost("{id}/close")]
    [HasPermission(Permissions.CloseShift)]
    [ProducesResponseType(typeof(CloseShiftResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Close([FromRoute] int id, [FromBody] CloseShiftRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CloseShiftCommand(id, request), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }
}
