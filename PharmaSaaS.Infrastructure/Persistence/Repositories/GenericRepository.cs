namespace PharmaSaaS.Infrastructure.Persistence.Repositories;

public class GenericRepository<TEntity>(ApplicationDbContext context) : IGenericRepository<TEntity> where TEntity : class
{
    private DbSet<TEntity>? _dbSet;
    protected DbSet<TEntity> DbSet => _dbSet ??= context.Set<TEntity>();

    #region Read

    #region Basic

    public async Task<TEntity?> GetAsync(object[] keyValues, CancellationToken cancellationToken = default)
        => await DbSet.FindAsync(keyValues, cancellationToken);

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IEnumerable<TEntity>> GetAllAsync(string[] includes, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().ApplyIncludesSafely(includes).ToListAsync(cancellationToken);

    public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, string[] includes, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().Where(predicate).ApplyIncludesSafely(includes).FirstOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public async Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, string[] includes, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().Where(predicate).ApplyIncludesSafely(includes).ToListAsync(cancellationToken);

    public async Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate, int? take = null, int? skip = null, Expression<Func<TEntity, object>>? orderBy = null, string orderByType = OrderBy.Ascending, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbSet.AsNoTracking().Where(predicate);

        if (orderBy != null)
            if (string.Equals(orderByType, OrderBy.Ascending, StringComparison.OrdinalIgnoreCase))
                query = query.OrderBy(orderBy);
            else if (string.Equals(orderByType, OrderBy.Descending, StringComparison.OrdinalIgnoreCase))
                query = query.OrderByDescending(orderBy);

        if (skip.HasValue)
            query = query.Skip(skip.Value);

        if (take.HasValue)
            query = query.Take(take.Value);

        return await query.ToListAsync(cancellationToken);
    }

    #endregion

    #region Tracked Find

    public async Task<TEntity?> TrackedFindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<TEntity?> TrackedFindAsync(Expression<Func<TEntity, bool>> predicate, string[] includes, CancellationToken cancellationToken = default)
        => await DbSet.Where(predicate).ApplyIncludesSafely(includes).FirstOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<TEntity>> TrackedFindAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await DbSet.Where(predicate).ToListAsync(cancellationToken);

    public async Task<IEnumerable<TEntity>> TrackedFindAllAsync(Expression<Func<TEntity, bool>> predicate, string[] includes, CancellationToken cancellationToken = default)
        => await DbSet.Where(predicate).ApplyIncludesSafely(includes).ToListAsync(cancellationToken);

    #endregion

    #region Projection

    public async Task<IEnumerable<TProjection>> GetAllProjectionAsync<TProjection>(CancellationToken cancellationToken = default) where TProjection : class
        => await DbSet.AsNoTracking().ProjectToType<TProjection>().ToListAsync(cancellationToken);

    public async Task<IEnumerable<TProjection>> GetAllProjectionAsync<TProjection>(string[] includes, CancellationToken cancellationToken = default) where TProjection : class
        => await DbSet.AsNoTracking().ApplyIncludesSafely(includes).ProjectToType<TProjection>().ToListAsync(cancellationToken);

    public async Task<IEnumerable<TProjection>> GetAllProjectionAsync<TProjection>(Expression<Func<TEntity, TProjection>> selector, bool distinct, CancellationToken cancellationToken = default)
        => distinct
            ? await DbSet.AsNoTracking().Select(selector).Distinct().ToListAsync(cancellationToken)
            : await DbSet.AsNoTracking().Select(selector).ToListAsync(cancellationToken);

    public async Task<IEnumerable<TProjection>> GetAllProjectionAsync<TProjection>(string[] includes, Expression<Func<TEntity, TProjection>> selector, bool distinct, CancellationToken cancellationToken = default)
        => distinct
            ? await DbSet.AsNoTracking().ApplyIncludesSafely(includes).Select(selector).Distinct().ToListAsync(cancellationToken)
            : await DbSet.AsNoTracking().ApplyIncludesSafely(includes).Select(selector).ToListAsync(cancellationToken);

    public async Task<IEnumerable<TProjection>> FindAllProjectionAsync<TProjection>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default) where TProjection : class
        => await DbSet.AsNoTracking().Where(predicate).ProjectToType<TProjection>().ToListAsync(cancellationToken);

    public async Task<IEnumerable<TProjection>> FindAllProjectionAsync<TProjection>(Expression<Func<TEntity, bool>> predicate, string[] includes, CancellationToken cancellationToken = default) where TProjection : class
        => await DbSet.AsNoTracking().Where(predicate).ApplyIncludesSafely(includes).ProjectToType<TProjection>().ToListAsync(cancellationToken);

    public async Task<IEnumerable<TProjection>> FindAllProjectionAsync<TProjection>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TProjection>> selector, bool distinct, CancellationToken cancellationToken = default)
        => distinct
            ? await DbSet.AsNoTracking().Where(predicate).Select(selector).Distinct().ToListAsync(cancellationToken)
            : await DbSet.AsNoTracking().Where(predicate).Select(selector).ToListAsync(cancellationToken);

    public async Task<IEnumerable<TProjection>> FindAllProjectionAsync<TProjection>(Expression<Func<TEntity, bool>> predicate, string[] includes, Expression<Func<TEntity, TProjection>> selector, bool distinct, CancellationToken cancellationToken = default)
        => distinct
            ? await DbSet.AsNoTracking().Where(predicate).ApplyIncludesSafely(includes).Select(selector).Distinct().ToListAsync(cancellationToken)
            : await DbSet.AsNoTracking().Where(predicate).ApplyIncludesSafely(includes).Select(selector).ToListAsync(cancellationToken);

    #endregion

    #endregion

    #region Modify

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
        return entities;
    }

    public void Delete(TEntity entity)
        => DbSet.Remove(entity);

    public void DeleteRange(IEnumerable<TEntity> entities)
        => DbSet.RemoveRange(entities);

    public async Task ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await DbSet.Where(predicate).ExecuteDeleteAsync(cancellationToken);

    public TEntity Update(TEntity entity)
    {
        DbSet.Update(entity);
        return entity;
    }

    public IEnumerable<TEntity> UpdateRange(IEnumerable<TEntity> entities)
    {
        DbSet.UpdateRange(entities);
        return entities;
    }

    public async Task<int> ExecuteUpdateAsync<TValue>(
        Expression<Func<TEntity, bool>> predicate,
        string propertyName, TValue newValue,
        CancellationToken cancellationToken = default)
        => await DbSet.Where(predicate)
            .ExecuteUpdateAsync(x => x
                .SetProperty(e => EF.Property<TValue>(e, propertyName), newValue)
            , cancellationToken);

    public async Task<int> ExecuteUpdateAsync<TValue1, TValue2>(
        Expression<Func<TEntity, bool>> predicate,
        string prop1, TValue1 val1,
        string prop2, TValue2 val2,
        CancellationToken cancellationToken = default)
        => await DbSet.Where(predicate)
            .ExecuteUpdateAsync(x => x
                .SetProperty(e => EF.Property<TValue1>(e, prop1), val1)
                .SetProperty(e => EF.Property<TValue2>(e, prop2), val2)
            , cancellationToken);

    public async Task<int> ExecuteUpdateAsync<TValue1, TValue2, TValue3>(
        Expression<Func<TEntity, bool>> predicate,
        string prop1, TValue1 val1,
        string prop2, TValue2 val2,
        string prop3, TValue3 val3,
        CancellationToken cancellationToken = default)
        => await DbSet.Where(predicate)
            .ExecuteUpdateAsync(x => x
                .SetProperty(e => EF.Property<TValue1>(e, prop1), val1)
                .SetProperty(e => EF.Property<TValue2>(e, prop2), val2)
                .SetProperty(e => EF.Property<TValue3>(e, prop3), val3)
            , cancellationToken);

    #endregion

    #region Helpers

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        => await DbSet.CountAsync(cancellationToken);

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await DbSet.CountAsync(predicate, cancellationToken);

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(predicate, cancellationToken);

    public async Task<bool> AnyWithWhereAsync(Expression<Func<TEntity, bool>> anyPredicate, Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking().Where(wherePredicate).AnyAsync(anyPredicate, cancellationToken);

    public void Attach(TEntity entity)
        => DbSet.Attach(entity);

    public void AttachRange(IEnumerable<TEntity> entities)
        => DbSet.AttachRange(entities);

    public void MarkPropertyAsModified<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> property)
    {
        var entry = context.Entry(entity);

        if (entry.State == EntityState.Detached)
            DbSet.Attach(entity);

        entry.Property(property).IsModified = true;
    }

    #endregion
}
