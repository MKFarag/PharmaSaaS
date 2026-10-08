using PharmaSaaS.Application.Features.AI.ParsePrescription;
using PharmaSaaS.Application.Features.AI.SetApiKey;
using PharmaSaaS.Application.Features.AI.SuggestMedications;
using PharmaSaaS.Domain.Errors;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// AI-powered prescription parsing and medication suggestion.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]
public class AIController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Parse a handwritten prescription image and extract drug names.
    /// </summary>
    /// <remarks>
    /// Accepts a prescription image (JPG, JPEG, PNG — max 5 MB) as multipart/form-data.
    /// The image is validated for file size, blocked signatures, and allowed extensions
    /// before being sent to the AI service.
    ///
    /// Each pharmacy receives 30 free calls per month. When the quota is exhausted,
    /// configure your own OpenRouter API key via PUT /api/AI/api-key.
    ///
    /// Sample request:
    ///
    ///     POST /api/AI/parse-prescription
    ///     Content-Type: multipart/form-data
    ///     image: [prescription.jpg]
    ///
    /// </remarks>
    /// <param name="request">The prescription image file (JPG, JPEG, or PNG, max 5 MB) wrapped in a request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of detected drug names.</returns>
    /// <response code="200">Returns the list of detected drug names.</response>
    /// <response code="400">If the image is missing, too large, has a blocked signature, or an invalid extension.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="429">If the monthly free quota is exhausted and no custom API key is configured.</response>
    /// <response code="503">If the AI service is temporarily unavailable.</response>
    [HttpPost("parse-prescription")]
    [HasPermission(Permissions.ParsePrescription)]
    [ProducesResponseType(typeof(AIResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AIRateLimitResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ParsePrescription([FromForm] ParsePrescriptionRequest request, CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();
        await request.Image.CopyToAsync(memoryStream, cancellationToken);
        var imageBytes = memoryStream.ToArray();

        var result = await sender.Send(new ParsePrescriptionCommand(imageBytes, request.Image.ContentType), cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.Error == AIErrors.MonthlyLimitReached)
        {
            var now = DateTime.UtcNow;
            var retryAfter = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
            return StatusCode(StatusCodes.Status429TooManyRequests, new AIRateLimitResponse(retryAfter));
        }

        return result.ToProblem();
    }

    /// <summary>
    /// Suggest medications based on a symptom description.
    /// </summary>
    /// <remarks>
    /// Accepts a free-text description in Arabic or English and returns suggested medication names.
    /// Uses the same monthly quota as prescription parsing.
    ///
    /// Sample request:
    ///
    ///     POST /api/AI/suggest-medications
    ///     {
    ///       "description": "شراب للكحة الجافة لطفل 5 سنين"
    ///     }
    ///
    /// </remarks>
    /// <param name="request">A symptom or need description in Arabic or English (max 500 characters).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of suggested drug names.</returns>
    /// <response code="200">Returns the list of suggested medications.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="429">If the monthly free quota is exhausted and no custom API key is configured.</response>
    /// <response code="503">If the AI service is temporarily unavailable.</response>
    [HttpPost("suggest-medications")]
    [HasPermission(Permissions.SuggestMedications)]
    [ProducesResponseType(typeof(AIResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(AIRateLimitResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SuggestMedications([FromBody] SuggestMedicationsRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SuggestMedicationsCommand(request), cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        if (result.Error == AIErrors.MonthlyLimitReached)
        {
            var now = DateTime.UtcNow;
            var retryAfter = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
            return StatusCode(StatusCodes.Status429TooManyRequests, new AIRateLimitResponse(retryAfter));
        }

        return result.ToProblem();
    }

    /// <summary>
    /// Set or clear your pharmacy's custom OpenRouter API key.
    /// </summary>
    /// <remarks>
    /// When the free monthly quota is exhausted, the system automatically falls back to this key.
    /// Send null or omit the key to remove the custom key and revert to platform-only mode.
    ///
    /// Sample request:
    ///
    ///     PUT /api/AI/api-key
    ///     {
    ///       "apiKey": "sk-or-v1-..."
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The OpenRouter API key to register (or null to clear).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the API key was successfully updated.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="404">If the pharmacy is not found.</response>
    [HttpPut("api-key")]
    [HasPermission(Permissions.SetAiApiKey)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetApiKey([FromBody] SetAiApiKeyRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SetAiApiKeyCommand(request), cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
