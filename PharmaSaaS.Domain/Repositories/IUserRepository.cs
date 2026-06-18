namespace PharmaSaaS.Domain.Repositories;

public interface IUserRepository
{
    #region Read Operations

    /// <summary>Finds a user by their unique identifier.</summary>
    Task<User?> FindByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Finds a user by their email address.</summary>
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Finds all users who have a specific role.</summary>
    Task<IEnumerable<User>> FindAllByRoleAsync(string roleName, CancellationToken cancellationToken = default);

    /// <summary>Gets all users with their roles projected to a specific type.</summary>
    Task<IEnumerable<TProjection>> GetAllProjectionWithRolesAsync<TProjection>(bool includeDefaultRole, CancellationToken cancellationToken = default)
        where TProjection : class;

    /// <summary>Gets both roles and permissions for a specific user.</summary>
    Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> GetRolesAndPermissionsAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>Gets the list of roles assigned to a user.</summary>
    Task<IList<string>> GetRolesAsync(User user);

    #endregion

    #region Check Operations

    /// <summary>Checks if a user account is currently locked out.</summary>
    Task<bool> IsLockedOutAsync(User user);

    /// <summary>Checks if a user's email address has been confirmed.</summary>
    Task<bool> IsEmailConfirmedAsync(User user);

    /// <summary>Checks if a user exists in the system.</summary>
    Task<bool> ExistsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Checks if an email address is already registered in the system.</summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Checks if an email address is already used by another user.</summary>
    Task<bool> EmailExistsAsync(string email, string userId, CancellationToken cancellationToken = default);

    #endregion

    #region Token & Code Operations

    /// <summary>Generates an email confirmation token for a user.</summary>
    Task<string> GenerateEmailConfirmationTokenAsync(User user);

    /// <summary>Confirms a user's email using a confirmation token.</summary>
    Task<Result> ConfirmEmailWithTokenAsync(User user, string token);

    /// <summary>Generates a token for changing a user's email address.</summary>
    Task<string> GenerateChangeEmailTokenAsync(User user, string newEmail);

    /// <summary>Changes a user's email address using a change email token.</summary>
    Task<Result> ChangeEmailAsync(User user, string newEmail, string token);

    /// <summary>Generates a password reset token for a user.</summary>
    Task<string> GeneratePasswordResetTokenAsync(User user);

    /// <summary>Resets a user's password using a reset token.</summary>
    Task<Result> ResetPasswordAsync(User user, string token, string newPassword);

    #endregion

    #region Modify Operations

    /// <summary>Creates a new user with the specified password.</summary>
    Task<Result> CreateAsync(User user, string password, bool confirmEmail = false);

    /// <summary>Updates an existing user's information.</summary>
    Task<Result> UpdateAsync(User user);

    /// <summary>Sets or removes a user's account lockout end date.</summary>
    Task<Result> SetLockoutEndDateAsync(User user, DateTimeOffset? lockoutEnd);

    /// <summary>Changes a user's password with current password verification.</summary>
    Task<Result> ChangePasswordAsync(User user, string currentPassword, string newPassword);

    /// <summary>Adds multiple roles to a user.</summary>
    Task<Result> AddToRolesAsync(User user, IEnumerable<string> roles);

    /// <summary>Adds a single role to a user.</summary>
    Task<Result> AddToRoleAsync(User user, string role);

    /// <summary>Removes all roles from a user in bulk operation.</summary>
    Task DeleteAllRolesAsync(User user);

    /// <summary>Adds a refresh token to a user's token collection.</summary>
    Task AddRefreshTokenAsync(User user, RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Revokes a user's refresh token by marking it as inactive.</summary>
    Task<Result> RevokeRefreshTokenAsync(User user, string token, CancellationToken cancellationToken = default);

    /// <summary>Removes all expired refresh tokens from the system.</summary>
    Task RemoveExpiredRefreshTokensAsync(CancellationToken cancellationToken = default);

    /// <summary>Toggle status for a specific user.</summary>
    Task ToggleStatusAsync(User user, CancellationToken cancellationToken = default);

    #endregion
}
