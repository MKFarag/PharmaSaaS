namespace PharmaSaaS.Domain.Errors;

public record UserErrors
{
    public static readonly Error NotFound =
        new("User.NotFound", "No user found", StatusCodes.NotFound);

    public static readonly Error InvalidCredentials =
        new("User.InvalidCredentials", "Invalid email/password", StatusCodes.Unauthorized);

    public static readonly Error InvalidToken =
        new("User.InvalidToken", "The token is invalid", StatusCodes.BadRequest);

    public static readonly Error InvalidCode =
        new("User.InvalidCode", "Invalid code", StatusCodes.Unauthorized);

    public static readonly Error InvalidJwtToken =
        new("User.InvalidJwtToken", "Invalid Jwt token", StatusCodes.Unauthorized);

    public static readonly Error DisabledUser =
        new("User.Disabled", "Disabled user, please contact your administrator", StatusCodes.Unauthorized);

    public static readonly Error LockedUser =
        new("User.Locked", "Locked user, please contact your administrator", StatusCodes.Unauthorized);

    public static readonly Error NotLockedUser =
        new("User.NotLocked", "This user is not locked", StatusCodes.BadRequest);

    public static readonly Error InvalidRefreshToken =
        new("User.InvalidRefreshToken", "Invalid refresh token", StatusCodes.Unauthorized);

    public static readonly Error DuplicatedEmail =
        new("User.DuplicatedEmail", "Another user with the same email is already exists", StatusCodes.Conflict);

    public static readonly Error SameEmail =
        new("User.SameEmail", "You cannot enter the same email", StatusCodes.BadRequest);

    public static readonly Error EmailNotConfirmed =
        new("User.EmailNotConfirmed", "Email is not confirmed", StatusCodes.Unauthorized);

    public static readonly Error DuplicatedConfirmation =
        new("User.DuplicatedConfirmation", "Email already confirmed", StatusCodes.BadRequest);

    public static readonly Error NoPermission =
        new("User.NoPermission", "You do not have permission to perform this action", StatusCodes.Forbidden);

    public static readonly Error InvalidRoles =
        new("User.InvalidRoles", "Invalid roles", StatusCodes.BadRequest);
}