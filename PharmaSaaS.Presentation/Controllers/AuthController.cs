using PharmaSaaS.Application.Contracts.Auth;
using PharmaSaaS.Application.Features.Auth.ConfirmEmail;
using PharmaSaaS.Application.Features.Auth.GetRefreshToken;
using PharmaSaaS.Application.Features.Auth.GetToken;
using PharmaSaaS.Application.Features.Auth.Register;
using PharmaSaaS.Application.Features.Auth.ResendConfirmationEmail;
using PharmaSaaS.Application.Features.Auth.ResetPassword;
using PharmaSaaS.Application.Features.Auth.RevokeRefreshToken;
using PharmaSaaS.Application.Features.Auth.SendResetPassword;

namespace PharmaSaaS.Presentation.Controllers;

/// <summary>
/// Manage user authentication and account access.
/// </summary>
[Route("[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitingOptions.PolicyNames.IpLimit)]
public class AuthController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Authenticates a user and generates tokens.
    /// </summary>
    /// <remarks>
    /// Logs in a user using their email and password, returning an access token and a refresh token.
    /// 
    /// Sample request:
    /// 
    ///     POST /Auth
    ///     {
    ///       "email": "user@example.com",
    ///       "password": "Password123!"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The login request credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated access and refresh tokens.</returns>
    /// <response code="200">Returns the authentication tokens.</response>
    /// <response code="400">If the credentials are invalid.</response>
    [HttpPost("")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new GetTokenCommand(request.Email, request.Password);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Refreshes an expired access token.
    /// </summary>
    /// <remarks>
    /// Generates a new access token using a valid refresh token.
    /// 
    /// Sample request:
    /// 
    ///     POST /Auth/refresh
    ///     {
    ///       "token": "eyJhbGciOiJIUz...",
    ///       "refreshToken": "d7j8s..."
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The request containing the expired access token and valid refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New access and refresh tokens.</returns>
    /// <response code="200">Returns the new tokens.</response>
    /// <response code="400">If the tokens are invalid or expired.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new GetRefreshTokenCommand(request.Token, request.RefreshToken);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.ToProblem();
    }

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    /// <remarks>
    /// Invalidates a specific refresh token so it can no longer be used.
    /// 
    /// Sample request:
    /// 
    ///     POST /Auth/revoke-refresh
    ///     {
    ///       "token": "eyJhbGciOiJIUz...",
    ///       "refreshToken": "d7j8s..."
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The request containing the access and refresh token to revoke.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ok on success.</returns>
    /// <response code="200">If the refresh token was successfully revoked.</response>
    /// <response code="400">If the tokens are invalid.</response>
    [HttpPost("revoke-refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevokeRefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new RevokeRefreshTokenCommand(request.Token, request.RefreshToken);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <remarks>
    /// Creates a new user account with the provided details.
    /// 
    /// Sample request:
    /// 
    ///     POST /Auth/register
    ///     {
    ///       "firstName": "John",
    ///       "lastName": "Doe",
    ///       "email": "user@example.com",
    ///       "password": "Password123!"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The user registration request details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ok on success.</returns>
    /// <response code="200">If the user was successfully registered.</response>
    /// <response code="400">If the registration data is invalid or email exists.</response>
    [HttpPost("register")]
    [Idempotent]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(request);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }

    /// <summary>
    /// Confirms a user's email address.
    /// </summary>
    /// <remarks>
    /// Verifies the user's email using a confirmation token sent during registration.
    /// 
    /// Sample request:
    /// 
    ///     POST /Auth/confirm
    ///     {
    ///       "userId": "123e4567-e89b-12d3-a456-426614174000",
    ///       "token": "CfDJ8..."
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The email confirmation request containing the user ID and token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ok on success.</returns>
    /// <response code="200">If the email was successfully confirmed.</response>
    /// <response code="400">If the token or user ID is invalid.</response>
    [HttpPost("confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var command = new ConfirmEmailCommand(request.UserId, request.Token);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }

    /// <summary>
    /// Resends the email confirmation code.
    /// </summary>
    /// <remarks>
    /// Resends a new email confirmation token to the specified email address if it's not already confirmed.
    /// 
    /// Sample request:
    /// 
    ///     POST /Auth/resend-confirm
    ///     {
    ///       "email": "user@example.com"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The request containing the user's email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ok on success.</returns>
    /// <response code="200">If the confirmation email was successfully resent.</response>
    /// <response code="400">If the email is invalid or already confirmed.</response>
    [HttpPost("resend-confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendConfirmEmail([FromBody] ResendConfirmationEmailRequest request, CancellationToken cancellationToken)
    {
        var command = new ResendConfirmationEmailCommand(request.Email);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }

    /// <summary>
    /// Initiates a password reset.
    /// </summary>
    /// <remarks>
    /// Sends a password reset code to the user's email address if they forgot their password.
    /// 
    /// Sample request:
    /// 
    ///     POST /Auth/forget-password
    ///     {
    ///       "email": "user@example.com"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The request containing the user's email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ok on success.</returns>
    /// <response code="200">If the reset password code was successfully sent.</response>
    /// <response code="400">If the email is invalid.</response>
    [HttpPost("forget-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new SendResetPasswordCommand(request.Email);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }

    /// <summary>
    /// Resets a user's password.
    /// </summary>
    /// <remarks>
    /// Verifies the reset token and sets a new password for the user.
    /// 
    /// Sample request:
    /// 
    ///     POST /Auth/reset-password
    ///     {
    ///       "email": "user@example.com",
    ///       "token": "CfDJ8...",
    ///       "newPassword": "NewPassword123!"
    ///     }
    /// 
    /// </remarks>
    /// <param name="request">The password reset request containing email, token, and new password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ok on success.</returns>
    /// <response code="200">If the password was successfully reset.</response>
    /// <response code="400">If the reset token or data is invalid.</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(request.Email, request.Token, request.NewPassword);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok()
            : result.ToProblem();
    }
}
