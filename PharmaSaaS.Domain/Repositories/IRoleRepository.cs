namespace PharmaSaaS.Domain.Repositories;

public interface IRoleRepository
{
    #region Read Operations

    /// <summary>Get a role by their unique identifier.</summary>
    Task<Role?> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Get all roles with optional filtering for default and disabled roles.</summary>
    Task<IEnumerable<Role>> GetAllAsync(bool includeDefault = false, bool includeDisabled = false, CancellationToken cancellationToken = default);

    /// <summary>Get all role names with optional filtering for default and disabled roles.</summary>
    Task<IEnumerable<string>> GetAllNamesAsync(bool includeDefault = false, bool includeDisabled = false, CancellationToken cancellationToken = default);

    /// <summary>Get all claim values for a specific role.</summary>
    Task<IEnumerable<string>> GetClaimsAsync(string roleId, CancellationToken cancellationToken = default);

    /// <summary>Get claim values for a specific role filtered by claim type.</summary>
    Task<IEnumerable<string>> GetClaimsAsync(string roleId, string claimType, CancellationToken cancellationToken = default);

    #endregion

    #region Check Operations

    /// <summary>Check if a role name already exists in the system.</summary>
    Task<bool> NameExistsAsync(string roleName, CancellationToken cancellationToken = default);

    /// <summary>Check if a role name exists excluding a specific role ID.</summary>
    Task<bool> NameExistsAsync(string roleName, string roleId, CancellationToken cancellationToken = default);

    #endregion

    #region Modify Operations

    /// <summary>Create a new role in the system.</summary>
    Task<Result> CreateAsync(Role role);

    /// <summary>Update an existing role in the system.</summary>
    Task<Result> UpdateAsync(Role role);

    /// <summary>Add a single claim to a role.</summary>
    Task AddClaimAsync(string roleId, string claimType, string claimValue, CancellationToken cancellationToken = default);

    /// <summary>Add multiple claims of the same type to a role.</summary>
    Task AddClaimsAsync(string roleId, string claimType, IEnumerable<string> claimValue, CancellationToken cancellationToken = default);

    /// <summary>Delete all claims for a specific role.</summary>
    Task DeleteClaimsAsync(string roleId, CancellationToken cancellationToken = default);

    /// <summary>Delete specific claims for a role based on claim values.</summary>
    Task DeleteClaimsAsync(string roleId, IEnumerable<string> claimValue, CancellationToken cancellationToken = default);

    /// <summary>Toggle status for a specific role.</summary>
    Task ToggleStatusAsync(Role role, CancellationToken cancellationToken = default);

    #endregion
}
