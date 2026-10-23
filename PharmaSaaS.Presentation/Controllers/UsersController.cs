using PharmaSaaS.Presentation.Attributes;
using PharmaSaaS.Application.Contracts.Users;
using PharmaSaaS.Application.Features.Users.Add;
using PharmaSaaS.Application.Features.Users.Get;
using PharmaSaaS.Application.Features.Users.GetAll;
using PharmaSaaS.Application.Features.Users.ToggleStatus;
using PharmaSaaS.Application.Features.Users.Update;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage users and their accounts.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Sliding)]
public class UsersController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves all users.
    /// </summary>
    /// <remarks>
    /// Returns a list of all registered users in the system.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of users.</returns>
    /// <response code="200">Returns the list of users.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="403">If the user does not have admin privileges.</response>
    [HttpGet("")]
    [HasPermission(Permissions.ReadUser)]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await sender.Send(new GetAllUsersQuery(), cancellationToken));

    /// <summary>
    /// Retrieves a specific user by ID.
    /// </summary>
    /// <remarks>
    /// Retrieves the details of a specific user based on their unique identifier.
    /// </remarks>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user details.</returns>
    /// <response code="200">Returns the user details.</response>
    /// <response code="404">If the user is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="403">If the user does not have admin privileges.</response>
    [HttpGet("{id}")]
    [HasPermission(Permissions.ReadUser)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get([FromRoute] string id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <remarks>
    /// Admins can use this to create new user accounts manually.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/Users
    ///     {
    ///       "firstName": "John",
    ///       "lastName": "Doe",
    ///       "email": "john.doe@example.com",
    ///       "password": "Password123!",
    ///       "roles": ["Admin", "Customer"]
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The user creation request details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created user details.</returns>
    /// <response code="201">Returns the newly created user.</response>
    /// <response code="400">If the request data is invalid or email already exists.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="403">If the user does not have admin privileges.</response>
    /// <response code="409">If the user's email is already exist'.</response>
    [HttpPost("")]
    [Idempotent]
    [HasPermission(Permissions.AddUser)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AddUserCommand(request), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <remarks>
    /// Admins can update a user's details and assigned roles.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/Users/123e4567-e89b-12d3-a456-426614174000
    ///     {
    ///       "firstName": "John",
    ///       "lastName": "Doe",
    ///       "email": "john.doe@example.com",
    ///       "roles": ["Customer"]
    ///     }
    /// 
    /// </remarks>
    /// <param name="id">The unique identifier of the user to update.</param>
    /// <param name="request">The updated user data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the user was successfully updated.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="404">If the user is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="403">If the user does not have admin privileges.</response>
    /// <response code="409">If the user's email is already exist'.</response>
    [HttpPut("{id}")]
    [HasPermission(Permissions.UpdateUser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateUserCommand(id, request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Toggles the active status of a user.
    /// </summary>
    /// <remarks>
    /// Enables or disables a user account. Disabled users cannot log in.
    /// </remarks>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the user status was successfully toggled.</response>
    /// <response code="404">If the user is not found.</response>
    /// <response code="401">If the user is unauthorized.</response>
    /// <response code="403">If the user does not have admin privileges.</response>
    [HttpPut("{id}/toggle-status")]
    [HasPermission(Permissions.ToggleUserStatus)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ToggleStatus([FromRoute] string id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ToggleUserStatusCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }
}
