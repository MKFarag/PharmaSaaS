using PharmaSaaS.Application.Contracts.Categories;
using PharmaSaaS.Application.Features.Categories.Add;
using PharmaSaaS.Application.Features.Categories.Delete;
using PharmaSaaS.Application.Features.Categories.Get;
using PharmaSaaS.Application.Features.Categories.GetAll;
using PharmaSaaS.Application.Features.Categories.Update;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage product categories.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]
public class CategoriesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves all categories.
    /// </summary>
    /// <remarks>
    /// Returns a list of all available product categories.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of categories.</returns>
    /// <response code="200">Returns the list of categories.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("")]
    [HasPermission(Permissions.ReadCategory)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAllCategoriesQuery(), cancellationToken));

    /// <summary>
    /// Retrieves a specific category by ID.
    /// </summary>
    /// <remarks>
    /// Retrieves the details of a specific category based on its unique identifier.
    /// </remarks>
    /// <param name="id">The unique identifier of the category.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The category details.</returns>
    /// <response code="200">Returns the category details.</response>
    /// <response code="404">If the category is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.ReadCategory)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoryQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <remarks>
    /// Creates a new product category with the specified name.
    ///
    /// Sample request:
    ///
    ///     POST /api/Categories
    ///     {
    ///       "name": "Antibiotics"
    ///     }
    ///
    /// </remarks>
    /// <param name="request">The category creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created category.</returns>
    /// <response code="201">Returns the newly created category.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If a category with the same name already exists.</response>
    [HttpPost("")]
    [HasPermission(Permissions.AddCategory)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromBody] CategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddCategoryCommand(request), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <remarks>
    /// Updates the name of an existing category by ID.
    ///
    /// Sample request:
    ///
    ///     PUT /api/Categories/1
    ///     {
    ///       "name": "Broad-Spectrum Antibiotics"
    ///     }
    ///
    /// </remarks>
    /// <param name="id">The unique identifier of the category to update.</param>
    /// <param name="request">The updated category data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the category was successfully updated.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="404">If the category is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If another category with the same name already exists.</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdateCategory)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateCategoryCommand(id, request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Deletes a category.
    /// </summary>
    /// <remarks>
    /// Deletes a category by its ID. Categories associated with one or more products cannot be deleted.
    /// </remarks>
    /// <param name="id">The unique identifier of the category to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the category was successfully deleted.</response>
    /// <response code="404">If the category is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="409">If the category is associated with one or more products.</response>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.DeleteCategory)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteCategoryCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}
