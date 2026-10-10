using PharmaSaaS.Application.Contracts.Loyalty;
using PharmaSaaS.Application.Features.Loyalty.AddRule;
using PharmaSaaS.Application.Features.Loyalty.AddTier;
using PharmaSaaS.Application.Features.Loyalty.DeleteRule;
using PharmaSaaS.Application.Features.Loyalty.DeleteTier;
using PharmaSaaS.Application.Features.Loyalty.GetAll;
using PharmaSaaS.Application.Features.Loyalty.UpdateRule;
using PharmaSaaS.Application.Features.Loyalty.UpdateTier;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage loyalty tiers and discount rules.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]
public class LoyaltyController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves all loyalty tiers.
    /// </summary>
    /// <remarks>
    /// Returns a list of all loyalty tiers and their discount rules.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of loyalty tiers.</returns>
    /// <response code="200">Returns the list of tiers.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("tiers")]
    [HasPermission(Permissions.ReadLoyalty)]
    [ProducesResponseType(typeof(IEnumerable<LoyaltyTierResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAllLoyaltyTiersQuery(), cancellationToken));

    /// <summary>
    /// Creates a new loyalty tier.
    /// </summary>
    /// <remarks>
    /// Creates a new loyalty tier.
    /// </remarks>
    /// <param name="request">The tier creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created tier id.</returns>
    /// <response code="201">Returns the newly created tier id.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If a tier with the same name already exists.</response>
    [HttpPost("tiers")]
    [HasPermission(Permissions.ManageLoyalty)]
    [ProducesResponseType(typeof(LoyaltyTierResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddTier([FromBody] LoyaltyTierRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddLoyaltyTierCommand(request), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), new { id = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Updates an existing loyalty tier.
    /// </summary>
    /// <remarks>
    /// Updates the details of an existing tier by ID.
    /// </remarks>
    /// <param name="id">The unique identifier of the tier to update.</param>
    /// <param name="request">The updated tier data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the tier was successfully updated.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="404">If the tier is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If another tier with the same name already exists.</response>
    [HttpPut("tiers/{id}")]
    [HasPermission(Permissions.ManageLoyalty)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateTier([FromRoute] int id, [FromBody] LoyaltyTierRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateLoyaltyTierCommand(id, request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Deletes a loyalty tier.
    /// </summary>
    /// <remarks>
    /// Deletes a loyalty tier by its ID.
    /// </remarks>
    /// <param name="id">The unique identifier of the tier to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the tier was successfully deleted.</response>
    /// <response code="404">If the tier is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If the tier is in use.</response>
    [HttpDelete("tiers/{id}")]
    [HasPermission(Permissions.ManageLoyalty)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteTier([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteLoyaltyTierCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Adds a discount rule to a tier.
    /// </summary>
    /// <remarks>
    /// Adds a discount rule to a loyalty tier.
    /// </remarks>
    /// <param name="tierId">The unique identifier of the tier.</param>
    /// <param name="request">The rule creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created rule id.</returns>
    /// <response code="201">Returns the newly created rule id.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="404">If the tier is not found.</response>
    [HttpPost("tiers/{tierId}/rules")]
    [HasPermission(Permissions.ManageLoyalty)]
    [ProducesResponseType(typeof(DiscountRuleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddRule([FromRoute] int tierId, [FromBody] DiscountRuleRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddDiscountRuleCommand(tierId, request), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), new { id = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Updates an existing discount rule.
    /// </summary>
    /// <remarks>
    /// Updates the details of an existing discount rule by ID.
    /// </remarks>
    /// <param name="ruleId">The unique identifier of the rule to update.</param>
    /// <param name="request">The updated rule data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the rule was successfully updated.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="404">If the rule is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpPut("rules/{ruleId}")]
    [HasPermission(Permissions.ManageLoyalty)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateRule([FromRoute] int ruleId, [FromBody] DiscountRuleRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateDiscountRuleCommand(ruleId, request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Deletes a discount rule.
    /// </summary>
    /// <remarks>
    /// Deletes a discount rule by its ID.
    /// </remarks>
    /// <param name="ruleId">The unique identifier of the rule to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the rule was successfully deleted.</response>
    /// <response code="404">If the rule is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpDelete("rules/{ruleId}")]
    [HasPermission(Permissions.ManageLoyalty)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteRule([FromRoute] int ruleId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteDiscountRuleCommand(ruleId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}
