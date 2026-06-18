using PharmaSaaS.Application.Contracts.Profile;
using PharmaSaaS.Application.Features.Profile.ChangeEmail;
using PharmaSaaS.Application.Features.Profile.ChangePassword;
using PharmaSaaS.Application.Features.Profile.ConfirmChangeEmail;
using PharmaSaaS.Application.Features.Profile.Get;
using PharmaSaaS.Application.Features.Profile.Update;

namespace PharmaSaaS.Presentation.Controllers;

[Route("me")]
[ApiController]
[Authorize]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.Sliding)]
public class ProfileController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Retrieves the authenticated user's profile.
    /// </summary>
    /// <remarks>
    /// Returns the profile information of the currently logged-in user.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's profile details.</returns>
    /// <response code="200">Returns the profile details.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the authenticated user is not found.</response>
    [HttpGet("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetMyProfileQuery(User.GetId()!), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Updates the authenticated user's profile.
    /// </summary>
    /// <remarks>
    /// Allows the currently logged-in user to update their first and last name.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/Profile
    ///     {
    ///       "firstName": "John",
    ///       "lastName": "Doe"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The updated profile data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the profile was successfully updated.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the authenticated user is not found.</response>
    [HttpPut("")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateProfileCommand(User.GetId()!, request), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Changes the authenticated user's password.
    /// </summary>
    /// <remarks>
    /// Allows the currently logged-in user to change their password by providing the current password and a new one.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/Profile/change-password
    ///     {
    ///       "currentPassword": "OldPassword123!",
    ///       "newPassword": "NewPassword456!"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The password change request containing the current and new passwords.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">If the password was successfully changed.</response>
    /// <response code="400">If the current password is incorrect or the new password is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the authenticated user is not found.</response>
    [HttpPut("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ChangeMyPasswordCommand(User.GetId()!, request.CurrentPassword, request.NewPassword), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.ToProblem();
    }

    /// <summary>
    /// Initiates an email change for the authenticated user.
    /// </summary>
    /// <remarks>
    /// Sends a confirmation link to the new email address. The change is not applied until the user confirms it via the confirm-change-email endpoint.
    /// 
    /// Sample request:
    /// 
    ///     PUT /api/Profile/change-email
    ///     {
    ///       "newEmail": "newemail@example.com"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The request containing the new email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ok on success (confirmation email has been sent).</returns>
    /// <response code="200">If the confirmation email was successfully sent.</response>
    /// <response code="400">If the new email is the same as the current one or is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the authenticated user is not found.</response>
    /// <response code="409">If the new email is already in use by another account.</response>
    [HttpPut("change-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ChangeMyEmailCommand(User.GetId()!, request.NewEmail), cancellationToken);

        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }

    /// <summary>
    /// Confirms the email change for the authenticated user.
    /// </summary>
    /// <remarks>
    /// Completes the email change process using the token sent to the new email address.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/Profile/confirm-change-email
    ///     {
    ///       "newEmail": "newemail@example.com",
    ///       "token": "CfDJ8..."
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The request containing the new email and the confirmation token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ok on success.</returns>
    /// <response code="200">If the email was successfully changed.</response>
    /// <response code="400">If the token is invalid or expired.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the authenticated user is not found.</response>
    [HttpPost("confirm-change-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmChangeEmail([FromBody] ConfirmChangeEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ConfirmChangeMyEmailCommand(User.GetId()!, request.NewEmail, request.Token), cancellationToken);

        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }
}
