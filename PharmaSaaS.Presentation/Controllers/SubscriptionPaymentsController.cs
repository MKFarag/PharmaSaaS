using PharmaSaaS.Application.Contracts.Payments;
using PharmaSaaS.Application.Features.Payments.GetAll;
using PharmaSaaS.Application.Features.Payments.Upload;
using PharmaSaaS.Application.Features.Payments.Verify;
using PharmaSaaS.Presentation.Contracts.Payments;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage subscription renewal payments.
/// </summary>
[Route("api/subscription-payments")]
[ApiController]
public class SubscriptionPaymentsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Uploads a subscription payment proof image for a pharmacy.
    /// </summary>
    /// <remarks>
    /// Public endpoint allowing pharmacy owners whose subscription has expired to submit a bank transfer or mobile wallet receipt.
    /// Does not require an active tenant session.
    /// </remarks>
    /// <param name="pharmacyId">The unique identifier of the pharmacy.</param>
    /// <param name="request">The payment proof image file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created status on success.</returns>
    /// <response code="201">If the payment proof was successfully uploaded.</response>
    /// <response code="400">If the image file is missing, exceeds size limits, or has an invalid type.</response>
    /// <response code="404">If the pharmacy is not found.</response>
    [HttpPost("{pharmacyId}")]
    [Consumes("multipart/form-data")]
    [NoTenantRequired]
    [EnableRateLimiting(RateLimitingOptions.PolicyNames.IpLimit)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload(
        [FromRoute] string pharmacyId,
        [FromForm] UploadPaymentRequest request,
        CancellationToken cancellationToken)
    {
        using var fileData = request.Image.ToFileData();

        var result = await sender.Send(new UploadSubscriptionPaymentCommand(pharmacyId, fileData), cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created)
            : result.ToProblem();
    }

    /// <summary>
    /// Retrieves all pending subscription payment proofs.
    /// </summary>
    /// <remarks>
    /// Returns a list of all unverified subscription payment receipts for administrative review. Requires system owner authorization.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of pending subscription payments.</returns>
    /// <response code="200">Returns the list of pending payments.</response>
    [HttpGet("")]
    [SystemOwner]
    [EnableRateLimiting(RateLimitingOptions.PolicyNames.Sliding)]
    [ProducesResponseType(typeof(IEnumerable<SubscriptionPaymentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPending(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAllSubscriptionPaymentsQuery(), cancellationToken));

    /// <summary>
    /// Verifies a subscription payment and extends the pharmacy subscription period.
    /// </summary>
    /// <remarks>
    /// Approves a payment proof, records the verified amount, and extends the pharmacy subscription by the specified number of months.
    /// Requires system owner authorization.
    ///
    /// Sample request:
    ///
    ///     PUT /api/subscription-payments/1/verify
    ///     {
    ///       "extendByMonths": 3,
    ///       "amount": 1500.00
    ///     }
    ///
    /// </remarks>
    /// <param name="id">The unique identifier of the subscription payment.</param>
    /// <param name="request">Verification details including months to extend and optional verified amount.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the payment was verified and subscription extended successfully.</response>
    /// <response code="400">If the request payload is invalid.</response>
    /// <response code="404">If the payment or associated pharmacy is not found.</response>
    /// <response code="409">If the payment has already been verified.</response>
    [HttpPut("{id}/verify")]
    [SystemOwner]
    [EnableRateLimiting(RateLimitingOptions.PolicyNames.Sliding)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Verify(
        [FromRoute] int id,
        [FromBody] VerifySubscriptionPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new VerifySubscriptionPaymentCommand(id, request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}
