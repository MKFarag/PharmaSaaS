using PharmaSaaS.Application.Contracts.Pharmacies;
using PharmaSaaS.Application.Contracts.SubscriptionRequests;
using PharmaSaaS.Application.Features.Pharmacies.Get;
using PharmaSaaS.Application.Features.Pharmacies.GetAll;
using PharmaSaaS.Application.Features.Pharmacies.SetSubscription;
using PharmaSaaS.Application.Features.Pharmacies.ToggleStatus;
using PharmaSaaS.Application.Features.Pharmacies.Update;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage pharmacy tenants in the platform.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[SystemOwner]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Sliding)]
public class PharmaciesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves all pharmacies.
    /// </summary>
    /// <remarks>
    /// Returns a list of all registered pharmacies in the platform.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of pharmacies.</returns>
    /// <response code="200">Returns the list of pharmacies.</response>
    [HttpGet("")]
    [ProducesResponseType(typeof(IEnumerable<PharmacyResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAllPharmaciesQuery(), cancellationToken));

    /// <summary>
    /// Retrieves a specific pharmacy by ID.
    /// </summary>
    /// <remarks>
    /// Retrieves the details of a specific pharmacy based on its unique identifier.
    /// </remarks>
    /// <param name="id">The unique identifier of the pharmacy.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The pharmacy details.</returns>
    /// <response code="200">Returns the pharmacy details.</response>
    /// <response code="404">If the pharmacy is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PharmacyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPharmacyQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Updates an existing pharmacy.
    /// </summary>
    /// <remarks>
    /// Updates the name of an existing pharmacy.
    ///
    /// Sample request:
    ///
    ///     PUT /api/Pharmacies/1
    ///     {
    ///       "name": "Updated Pharmacy Name"
    ///     }
    ///
    /// </remarks>
    /// <param name="id">The unique identifier of the pharmacy to update.</param>
    /// <param name="request">The updated pharmacy data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the pharmacy was successfully updated.</response>
    /// <response code="404">If the pharmacy is not found.</response>
    /// <response code="409">If another pharmacy with the same name exists under this tenant.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] PharmacyRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdatePharmacyCommand(id, request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Toggles the status of a pharmacy.
    /// </summary>
    /// <remarks>
    /// Enables or disables a pharmacy by its ID. Disabled pharmacies block write operations.
    /// </remarks>
    /// <param name="id">The unique identifier of the pharmacy.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the pharmacy status was successfully toggled.</response>
    /// <response code="404">If the pharmacy is not found.</response>
    [HttpPut("toggle-status/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleStatus([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new TogglePharmacyStatusCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Sets the subscription end date for a pharmacy.
    /// </summary>
    /// <remarks>
    /// Sets the end date for a pharmacy's active subscription period.
    ///
    /// Sample request:
    ///
    ///     PUT /api/Pharmacies/1/subscription
    ///     {
    ///       "end": "2027-01-01"
    ///     }
    ///
    /// </remarks>
    /// <param name="id">The unique identifier of the pharmacy.</param>
    /// <param name="request">The subscription end date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the subscription was successfully set.</response>
    /// <response code="404">If the pharmacy is not found.</response>
    [HttpPut("{id}/subscription")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetSubscription([FromRoute] int id, [FromBody] SetSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetPharmacySubscriptionCommand(id, request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}
