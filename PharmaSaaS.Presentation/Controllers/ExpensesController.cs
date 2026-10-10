using PharmaSaaS.Application.Contracts.Expenses;
using PharmaSaaS.Application.Features.Expenses.Add;
using PharmaSaaS.Application.Features.Expenses.Delete;
using PharmaSaaS.Application.Features.Expenses.GetAllByShift;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage expenses.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]
public class ExpensesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Log a new expense.
    /// </summary>
    /// <remarks>
    /// Logs a new expense under the currently active shift.
    ///
    /// Sample request:
    ///
    ///     POST /api/Expenses
    ///     {
    ///       "amount": 100.50,
    ///       "description": "Internet Bill"
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The expense creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly logged expense.</returns>
    /// <response code="201">Returns the newly logged expense.</response>
    /// <response code="400">If the request data is invalid or shift is not open.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpPost]
    [HasPermission(Permissions.LogExpense)]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Log([FromBody] ExpenseRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new LogExpenseCommand(request), cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(GetAllExpensesByShift), new { shiftId = result.Value.ShiftId }, result.Value) : result.ToProblem();
    }

    /// <summary>
    /// Retrieves all expenses for a specific shift.
    /// </summary>
    /// <remarks>
    /// Returns a list of all logged expenses for the given shift ID.
    /// </remarks>
    /// <param name="shiftId">The unique identifier of the shift.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of expenses for the shift.</returns>
    /// <response code="200">Returns the list of expenses.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("shift/{shiftId}")]
    [HasPermission(Permissions.ReadExpense)]
    [ProducesResponseType(typeof(IEnumerable<ExpenseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllExpensesByShift([FromRoute] int shiftId, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAllExpensesByShiftQuery(shiftId), cancellationToken));

    /// <summary>
    /// Deletes an expense.
    /// </summary>
    /// <remarks>
    /// Deletes an expense by its ID. Only allowed if the linked shift is still open.
    /// </remarks>
    /// <param name="id">The unique identifier of the expense to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the expense was successfully deleted.</response>
    /// <response code="400">If the linked shift is closed.</response>
    /// <response code="404">If the expense is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.DeleteExpense)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteExpenseCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
