using PharmaSaaS.Application.Contracts.Suppliers;
using PharmaSaaS.Application.Features.Suppliers.Add;
using PharmaSaaS.Application.Features.Suppliers.Delete;
using PharmaSaaS.Application.Features.Suppliers.Get;
using PharmaSaaS.Application.Features.Suppliers.GetAll;
using PharmaSaaS.Application.Features.Suppliers.ResetBalance;
using PharmaSaaS.Application.Features.Suppliers.Update;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage suppliers.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]
public class SuppliersController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves all suppliers.
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of all suppliers registered under the pharmacy.
    /// Includes the outstanding balance and the date of the last order per supplier.
    ///
    /// Sample request:
    ///
    ///     GET /api/suppliers
    ///
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of suppliers.</returns>
    /// <response code="200">Returns the list of suppliers.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("")]
    [HasPermission(Permissions.ReadSupplier)]
    [ProducesResponseType(typeof(IEnumerable<SupplierResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAllSuppliersQuery(), cancellationToken));

    /// <summary>
    /// Retrieves a specific supplier by ID.
    /// </summary>
    /// <remarks>
    /// Returns the supplier details.
    ///
    /// Sample request:
    ///
    ///     GET /api/suppliers/1
    ///
    /// </remarks>
    /// <param name="id">The unique identifier of the supplier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The supplier details.</returns>
    /// <response code="200">Returns the supplier details.</response>
    /// <response code="404">If the supplier is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.ReadSupplier)]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSupplierByIdQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Registers a new supplier.
    /// </summary>
    /// <remarks>
    /// Creates a new supplier under the pharmacy. Supplier names must be unique.
    ///
    /// Sample request:
    ///
    ///     POST /api/suppliers
    ///     {
    ///       "name": "MedPharm Distributors",
    ///       "contactInfo": "+20 100 000 0000 | contact@medpharm.com"
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The supplier creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created supplier.</returns>
    /// <response code="201">Returns the newly created supplier.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If a supplier with the same name already exists.</response>
    [HttpPost("")]
    [HasPermission(Permissions.AddSupplier)]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromBody] SupplierRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddSupplierCommand(request), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Updates an existing supplier.
    /// </summary>
    /// <remarks>
    /// Updates the name and/or contact info of a supplier.
    ///
    /// Sample request:
    ///
    ///     PUT /api/suppliers/1
    ///     {
    ///       "name": "Updated Distributor Name",
    ///       "contactInfo": "+20 111 000 0000"
    ///     }
    ///
    /// </remarks>
    /// <param name="id">The unique identifier of the supplier to update.</param>
    /// <param name="request">The updated supplier data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the supplier was successfully updated.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="404">If the supplier is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If another supplier with the same name already exists.</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdateSupplier)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] SupplierRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateSupplierCommand(id, request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Deletes a supplier.
    /// </summary>
    /// <remarks>
    /// Deletes a supplier by ID. Deletion is blocked if the supplier has linked purchase orders.
    ///
    /// Sample request:
    ///
    ///     DELETE /api/suppliers/1
    ///
    /// </remarks>
    /// <param name="id">The unique identifier of the supplier to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the supplier was successfully deleted.</response>
    /// <response code="404">If the supplier is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If the supplier has linked purchase orders.</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.DeleteSupplier)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteSupplierCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Resets the outstanding balance for a supplier.
    /// </summary>
    /// <remarks>
    /// Sets the supplier's balance back to zero, indicating all debts have been settled.
    ///
    /// Sample request:
    ///
    ///     PATCH /api/suppliers/1/reset-balance
    ///
    /// </remarks>
    /// <param name="id">The unique identifier of the supplier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the balance was successfully reset.</response>
    /// <response code="404">If the supplier is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpPatch("{id}/reset-balance")]
    [HasPermission(Permissions.ResetSupplierBalance)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResetBalance([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ResetSupplierBalanceCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}
