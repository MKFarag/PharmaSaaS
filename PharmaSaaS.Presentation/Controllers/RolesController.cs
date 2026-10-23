using PharmaSaaS.Presentation.Attributes;
using PharmaSaaS.Application.Contracts.Roles;
using PharmaSaaS.Application.Features.Roles.Add;
using PharmaSaaS.Application.Features.Roles.Get;
using PharmaSaaS.Application.Features.Roles.GetAll;
using PharmaSaaS.Application.Features.Roles.GetAllPermissions;
using PharmaSaaS.Application.Features.Roles.ToggleStatus;
using PharmaSaaS.Application.Features.Roles.Update;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage user roles and permissions.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Concurrency)]
public class RolesController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves all roles.
    /// </summary>
    /// <remarks>
    /// Returns a list of all available roles. You can optionally include disabled roles.
    /// </remarks>
    /// <param name="includeDisabled">A boolean flag to include disabled roles in the response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of roles.</returns>
    /// <response code="200">Returns the list of roles.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="403">If the user does not have permission to read roles.</response>
    [HttpGet("")]
    [HasPermission(Permissions.ReadRole)]
    [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeDisabled = false, CancellationToken cancellationToken = default)
        => Ok(await sender.Send(new GetAllRolesQuery(includeDisabled), cancellationToken));

    /// <summary>
    /// Retrieves all system permissions.
    /// </summary>
    /// <remarks>
    /// Returns a list of all available system permissions that can be assigned to roles.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of permissions.</returns>
    /// <response code="200">Returns the list of permissions.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="403">If the user does not have permission to read roles.</response>
    [HttpGet("permissions")]
    [HasPermission(Permissions.ReadRole)]
    [ProducesResponseType(typeof(IEnumerable<PermissionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllPermissions(CancellationToken cancellationToken = default)
        => Ok(await sender.Send(new GetAllPermissionsQuery(), cancellationToken));

    /// <summary>
    /// Retrieves a specific role by ID.
    /// </summary>
    /// <remarks>
    /// Retrieves the details of a specific role based on its unique identifier.
    /// </remarks>
    /// <param name="id">The unique identifier of the role.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The role details.</returns>
    /// <response code="200">Returns the role details.</response>
    /// <response code="404">If the role is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="403">If the user does not have permission to read roles.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.ReadRole)]
    [ProducesResponseType(typeof(RoleDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get([FromRoute] string id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRoleQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <remarks>
    /// Creates a new role with the specified permissions.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/Roles
    ///     {
    ///       "name": "Manager",
    ///       "permissions": ["ReadOrder", "UpdateOrder"]
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The role creation request containing name and permissions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created role details.</returns>
    /// <response code="201">Returns the newly created role.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="403">If the user does not have permission to add roles.</response>
    /// <response code="409">If a role with the same name already exists.</response>
    [HttpPost("")]
    [Idempotent]
    [HasPermission(Permissions.AddRole)]
    [ProducesResponseType(typeof(RoleDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromBody] RoleRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddRoleCommand(request), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Updates an existing role.
    /// </summary>
    /// <remarks>
    /// Updates the details and permissions of an existing role by ID.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/Roles/1
    ///     {
    ///       "name": "Manager",
    ///       "permissions": ["ReadOrder", "UpdateOrder", "DeleteOrder"]
    ///     }
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the role to update.</param>
    /// <param name="request">The updated role data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the role was successfully updated.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="404">If the role is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="403">If the user does not have permission to update roles.</response>
    /// <response code="409">If a role with the same name already exists.</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdateRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] RoleRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateRoleCommand(id, request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Toggles the status of a role.
    /// </summary>
    /// <remarks>
    /// Enables or disables a role by its ID. Disabled roles might not be applicable to users.
    /// </remarks>
    /// <param name="id">The unique identifier of the role.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the role status was successfully toggled.</response>
    /// <response code="404">If the role is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="403">If the user does not have permission to toggle role status.</response>
    [HttpPut("toggle-status/{id}")]
    [HasPermission(Permissions.ToggleRoleStatus)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ToggleStatus([FromRoute] string id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ToggleRoleStatusCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}
