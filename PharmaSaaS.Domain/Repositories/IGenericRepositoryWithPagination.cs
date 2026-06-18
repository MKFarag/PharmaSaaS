namespace PharmaSaaS.Domain.Repositories;

public interface IGenericRepositoryWithPagination<TEntity>
    : IGenericRepository<TEntity>, IPaginatedRepository<TEntity>
    where TEntity : class
{
}
