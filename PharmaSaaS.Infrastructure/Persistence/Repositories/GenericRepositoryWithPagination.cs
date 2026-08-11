namespace PharmaSaaS.Infrastructure.Persistence.Repositories;

public class GenericRepositoryWithPagination<TEntity>(ApplicationDbContext context)
    : GenericRepository<TEntity>(context), IGenericRepositoryWithPagination<TEntity> where TEntity : class
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public async Task<IPaginatedList<TProjection>> GetPaginatedListAsync<TProjection>(
        int pageNumber, int pageSize, string defaultSortColumn, CancellationToken cancellationToken = default) where TProjection : class
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        query = query.OrderBy(defaultSortColumn, OrderBy.Ascending);

        return await PaginatedList<TProjection>.CreateAsync(query.ProjectToType<TProjection>(), pageNumber, pageSize, cancellationToken);
    }

    public async Task<IPaginatedList<TProjection>> GetPaginatedListAsync<TProjection>(
        int pageNumber, int pageSize, string defaultSortColumn, string[] includes, CancellationToken cancellationToken = default) where TProjection : class
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        query = query
            .ApplyIncludesSafely(includes)
            .OrderBy(defaultSortColumn, OrderBy.Ascending);

        return await PaginatedList<TProjection>.CreateAsync(query.ProjectToType<TProjection>(), pageNumber, pageSize, cancellationToken);
    }

    public async Task<IPaginatedList<TProjection>> GetPaginatedListAsync<TProjection>(
        int pageNumber, int pageSize, string? searchValue, string? searchColumn, string sortColumn, string sortDirection,
        ColumnType searchColumnType, CancellationToken cancellationToken = default) where TProjection : class
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        query = query
            .ApplySearchFilter(searchValue, searchColumn, searchColumnType)
            .OrderBy(sortColumn, sortDirection);

        return await PaginatedList<TProjection>.CreateAsync(query.ProjectToType<TProjection>(), pageNumber, pageSize, cancellationToken);
    }

    public async Task<IPaginatedList<TProjection>> GetPaginatedListAsync<TProjection>(
        int pageNumber, int pageSize, string? searchValue, string? searchColumn, string sortColumn, string sortDirection,
        ColumnType searchColumnType, string[] includes, CancellationToken cancellationToken = default) where TProjection : class
    {
        var query = _dbSet.AsNoTracking().AsQueryable();

        query = query
            .ApplySearchFilter(searchValue, searchColumn, searchColumnType)
            .ApplyIncludesSafely(includes)
            .OrderBy(sortColumn, sortDirection);

        return await PaginatedList<TProjection>.CreateAsync(query.ProjectToType<TProjection>(), pageNumber, pageSize, cancellationToken);
    }

    public async Task<IPaginatedList<TProjection>> FindPaginatedListAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        int pageNumber, int pageSize, string defaultSortColumn, CancellationToken cancellationToken = default) where TProjection : class
    {
        var query = _dbSet.AsNoTracking().Where(predicate).AsQueryable();

        query = query.OrderBy(defaultSortColumn, OrderBy.Ascending);

        return await PaginatedList<TProjection>.CreateAsync(query.ProjectToType<TProjection>(), pageNumber, pageSize, cancellationToken);
    }

    public async Task<IPaginatedList<TProjection>> FindPaginatedListAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        int pageNumber, int pageSize, string defaultSortColumn, string[] includes, CancellationToken cancellationToken = default) where TProjection : class
    {
        var query = _dbSet.AsNoTracking().Where(predicate).AsQueryable();

        query = query
            .ApplyIncludesSafely(includes)
            .OrderBy(defaultSortColumn, OrderBy.Ascending);

        return await PaginatedList<TProjection>.CreateAsync(query.ProjectToType<TProjection>(), pageNumber, pageSize, cancellationToken);
    }

    public async Task<IPaginatedList<TProjection>> FindPaginatedListAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        int pageNumber, int pageSize, string? searchValue, string? searchColumn, string sortColumn, string sortDirection,
        ColumnType searchColumnType, CancellationToken cancellationToken = default) where TProjection : class
    {
        var query = _dbSet.AsNoTracking().Where(predicate).AsQueryable();

        query = query
            .ApplySearchFilter(searchValue, searchColumn, searchColumnType)
            .OrderBy(sortColumn, sortDirection);

        return await PaginatedList<TProjection>.CreateAsync(query.ProjectToType<TProjection>(), pageNumber, pageSize, cancellationToken);
    }

    public async Task<IPaginatedList<TProjection>> FindPaginatedListAsync<TProjection>(
        Expression<Func<TEntity, bool>> predicate,
        int pageNumber, int pageSize, string? searchValue, string? searchColumn, string sortColumn, string sortDirection,
        ColumnType searchColumnType, string[] includes, CancellationToken cancellationToken = default) where TProjection : class
    {
        var query = _dbSet.AsNoTracking().Where(predicate).AsQueryable();

        query = query
            .ApplySearchFilter(searchValue, searchColumn, searchColumnType)
            .ApplyIncludesSafely(includes)
            .OrderBy(sortColumn, sortDirection);

        return await PaginatedList<TProjection>.CreateAsync(query.ProjectToType<TProjection>(), pageNumber, pageSize, cancellationToken);
    }
}