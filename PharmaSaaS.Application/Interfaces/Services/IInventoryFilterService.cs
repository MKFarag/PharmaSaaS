namespace PharmaSaaS.Application.Interfaces.Services;

/// <summary>
/// Builds the filter predicate and resolves sort options for inventory list queries,
/// keeping the query handler free of multi-condition expression logic.
/// </summary>
public interface IInventoryFilterService
{
    /// <summary>
    /// Builds a composited LINQ predicate from all active filters in <paramref name="filters"/>.
    /// </summary>
    /// <param name="filters">The caller-supplied inventory request filters.</param>
    /// <returns>A compiled <see cref="Expression{TDelegate}"/> ready to be passed to the repository.</returns>
    Expression<Func<Inventory, bool>> BuildPredicate(InventoryRequestFilters filters);

    /// <summary>
    /// Resolves the sort column name and sort direction from <paramref name="filters"/>.
    /// </summary>
    /// <param name="filters">The caller-supplied inventory request filters.</param>
    /// <returns>A tuple of <c>(sortColumn, sortDirection)</c> using <see cref="OrderBy"/> constants.</returns>
    (string SortColumn, string SortDirection) ResolveSorting(InventoryRequestFilters filters);
}
