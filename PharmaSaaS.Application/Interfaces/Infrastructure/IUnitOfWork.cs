namespace PharmaSaaS.Application.Interfaces.Infrastructure;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }

    /// <summary>Save changes to the database</summary>
    /// <returns>The number of state entries written to the database</returns>
    int Complete();

    /// <summary>Save changes to the database</summary>
    /// <returns>The number of state entries written to the database</returns>
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);

    /// <summary>Starts a database transaction if one is not already active</summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Commits the current active database transaction</summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Rolls back the current active database transaction</summary>
    Task RollbackTransactionAsync();
}
