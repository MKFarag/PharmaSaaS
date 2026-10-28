using PharmaSaaS.Application.Contracts.SubscriptionRequests;
using PharmaSaaS.Application.Features.SubscriptionRequests.Create;
using PharmaSaaS.Application.Features.SubscriptionRequests.Get;
using PharmaSaaS.Application.Features.SubscriptionRequests.GetAll;
using PharmaSaaS.Application.Features.SubscriptionRequests.UpdateStatus;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manages subscription requests for pharmacy onboarding.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.IpLimit)]
public class SubscriptionRequestsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Submits a new subscription request.
    /// </summary>
    /// <remarks>
    /// Public endpoint allowing prospective pharmacy owners to submit an onboarding request.
    ///
    /// Sample request:
    ///
    ///     POST /api/SubscriptionRequests
    ///     {
    ///       "pharmacyName": "My Pharmacy",
    ///       "ownerName": "John Doe",
    ///       "phoneNumber": "+201234567890",
    ///       "email": "john@example.com"
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The subscription request details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the subscription request was successfully submitted.</response>
    [HttpPost]
    [Idempotent]
    [NoTenantRequired]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Submit([FromBody] CreateSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SubmitSubscriptionRequestCommand(request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Retrieves all subscription requests.
    /// </summary>
    /// <remarks>
    /// Returns a list of all subscription requests. Requires system owner authorization.
    /// </remarks>
    /// <param name="status">Optional status filter (Pending, Approved, Rejected, Contacted).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of subscription requests.</returns>
    /// <response code="200">Returns the list of subscription requests.</response>
    [HttpGet]
    [SystemOwner]
    [EnableRateLimiting(RateLimitingOptions.PolicyNames.Sliding)]
    [ProducesResponseType(typeof(IEnumerable<SubscriptionRequestResponse>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] SubscriptionRequestStatus? status, CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAllSubscriptionRequestsQuery(status), cancellationToken));

    /// <summary>
    /// Retrieves a specific subscription request by ID.
    /// </summary>
    /// <remarks>
    /// Retrieves the details of a specific subscription request. Requires system owner authorization.
    /// </remarks>
    /// <param name="id">The unique identifier of the subscription request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The subscription request details.</returns>
    /// <response code="200">Returns the subscription request details.</response>
    /// <response code="404">If the subscription request is not found.</response>
    [HttpGet("{id}")]
    [SystemOwner]
    [EnableRateLimiting(RateLimitingOptions.PolicyNames.Sliding)]
    [ProducesResponseType(typeof(SubscriptionRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] string id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSubscriptionRequestQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Updates the status of a subscription request.
    /// </summary>
    /// <remarks>
    /// Updates the status of an existing subscription request. When set to "Approved", a new pharmacy is automatically created.
    /// Requires system owner authorization.
    ///
    /// Sample request:
    ///
    ///     PUT /api/SubscriptionRequests/{id}/status
    ///     {
    ///       "status": "Approved",
    ///       "durationInMonths": 1
    ///     }
    ///
    /// </remarks>
    /// <param name="id">The unique identifier of the subscription request.</param>
    /// <param name="request">The new status value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the subscription request status was successfully updated.</response>
    /// <response code="404">If the subscription request is not found.</response>
    [HttpPut("{id}/status")]
    [SystemOwner]
    [EnableRateLimiting(RateLimitingOptions.PolicyNames.Sliding)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus([FromRoute] string id, [FromBody] UpdateSubscriptionRequestStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateSubscriptionRequestStatusCommand(id, request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}
