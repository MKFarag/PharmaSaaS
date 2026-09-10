namespace PharmaSaaS.Application.Services;

/// <summary>
/// Translates <see cref="InventoryRequestFilters"/> into a repository-ready LINQ predicate
/// and sort descriptor.
///
/// Each filter flag is composed with a short-circuit guard so that null / false flags are
/// completely transparent — they contribute no additional WHERE clause to the query.
///
/// Filter semantics:
///   IsRare          — exact match on <see cref="Inventory.IsRare"/>.
///   OutOfStock      — true ⇒ Quantity == 0 only.
///   BelowTarget     — true  ⇒ Quantity &lt; TargetStock (requires TargetStock set).
///                     false ⇒ Quantity >= TargetStock (requires TargetStock set).
///   NotReordered    — true ⇒ LastSoldAt is set AND (LastOrderedAt is null OR LastOrderedAt &lt; LastSoldAt).
///   SoldSince       — LastSoldAt >= supplied date.
///   LastOrderedBefore — LastOrderedAt is null OR LastOrderedAt &lt; supplied date.
/// </summary>
public sealed class InventoryFilterService : IInventoryFilterService
{
    public Expression<Func<Inventory, bool>> BuildPredicate(InventoryRequestFilters filters)
        => i =>
            (!filters.IsRare.HasValue || i.IsRare == filters.IsRare.Value) &&
            (filters.OutOfStock != true || i.Quantity == 0) &&
            (!filters.BelowTarget.HasValue ||
                (i.TargetStock.HasValue &&
                    ((filters.BelowTarget == true && i.Quantity < i.TargetStock.Value) ||
                     (filters.BelowTarget == false && i.Quantity >= i.TargetStock.Value)))) &&
            (filters.NotReordered != true ||
                (i.LastSoldAt.HasValue &&
                    (i.LastOrderedAt == null || i.LastOrderedAt < i.LastSoldAt))) &&
            (!filters.SoldSince.HasValue ||
                (i.LastSoldAt.HasValue && i.LastSoldAt >= filters.SoldSince.Value)) &&
            (!filters.LastOrderedBefore.HasValue ||
                i.LastOrderedAt == null || i.LastOrderedAt < filters.LastOrderedBefore.Value);

    public (string SortColumn, string SortDirection) ResolveSorting(InventoryRequestFilters filters)
        => filters.SortByMostSold == true
            ? (nameof(Inventory.TotalUnitsSold), OrderBy.Descending)
            : (nameof(Inventory.Id), OrderBy.Ascending);
}
