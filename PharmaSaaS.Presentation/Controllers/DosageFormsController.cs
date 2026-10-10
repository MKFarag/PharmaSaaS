using PharmaSaaS.Application.Contracts.DosageForms;
using PharmaSaaS.Application.Features.DosageForms.Add;
using PharmaSaaS.Application.Features.DosageForms.Delete;
using PharmaSaaS.Application.Features.DosageForms.Get;
using PharmaSaaS.Application.Features.DosageForms.GetAll;
using PharmaSaaS.Application.Features.DosageForms.Update;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage product dosage forms.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]
public class DosageFormsController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves all dosage forms.
    /// </summary>
    /// <remarks>
    /// Returns a list of all available product dosage forms.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of dosage forms.</returns>
    /// <response code="200">Returns the list of dosage forms.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("")]
    [HasPermission(Permissions.ReadDosageForm)]
    [ProducesResponseType(typeof(IEnumerable<DosageFormResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAllDosageFormsQuery(), cancellationToken));

    /// <summary>
    /// Retrieves a specific dosage form by ID.
    /// </summary>
    /// <remarks>
    /// Retrieves the details of a specific dosage form based on its unique identifier.
    /// </remarks>
    /// <param name="id">The unique identifier of the dosage form.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The dosage form details.</returns>
    /// <response code="200">Returns the dosage form details.</response>
    /// <response code="404">If the dosage form is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.ReadDosageForm)]
    [ProducesResponseType(typeof(DosageFormResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetDosageFormQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Creates a new dosage form.
    /// </summary>
    /// <remarks>
    /// Creates a new product dosage form with the specified name.
    ///
    /// Sample request:
    ///
    ///     POST /api/DosageForms
    ///     {
    ///       "name": "Tablet"
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The dosage form creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created dosage form.</returns>
    /// <response code="201">Returns the newly created dosage form.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If a dosage form with the same name already exists.</response>
    [HttpPost("")]
    [HasPermission(Permissions.AddDosageForm)]
    [ProducesResponseType(typeof(DosageFormResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromBody] DosageFormRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddDosageFormCommand(request), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Updates an existing dosage form.
    /// </summary>
    /// <remarks>
    /// Updates the name of an existing dosage form by ID.
    ///
    /// Sample request:
    ///
    ///     PUT /api/DosageForms/1
    ///     {
    ///       "name": "Film-Coated Tablet"
    ///     }
    ///
    /// </remarks>
    /// <param name="id">The unique identifier of the dosage form to update.</param>
    /// <param name="request">The updated dosage form data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the dosage form was successfully updated.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="404">If the dosage form is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If another dosage form with the same name already exists.</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdateDosageForm)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] DosageFormRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateDosageFormCommand(id, request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Deletes a dosage form.
    /// </summary>
    /// <remarks>
    /// Deletes a dosage form by its ID. Dosage forms associated with one or more products cannot be deleted.
    /// </remarks>
    /// <param name="id">The unique identifier of the dosage form to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the dosage form was successfully deleted.</response>
    /// <response code="404">If the dosage form is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If the dosage form is associated with one or more products.</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.DeleteDosageForm)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteDosageFormCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}
