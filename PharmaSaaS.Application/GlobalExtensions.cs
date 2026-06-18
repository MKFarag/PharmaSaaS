namespace PharmaSaaS.Application;

internal static class GlobalExtensions
{
    extension(RequestFilters filters)
    {
        internal RequestFilters Check(HashSet<string> allowedSortColumns, HashSet<string> allowedSearchColumns)
        {
            string? searchColumn = string.Empty;
            string sortColumn, sortDirection;

            if (!string.IsNullOrEmpty(filters.SearchValue))
                searchColumn = allowedSearchColumns
                    .FirstOrDefault(x => string.Equals(x, filters.SearchColumn, StringComparison.OrdinalIgnoreCase))
                    ?? allowedSearchColumns.First();

            if (!string.IsNullOrEmpty(filters.SortColumn))
                sortColumn = allowedSortColumns
                    .FirstOrDefault(x => string.Equals(x, filters.SortColumn, StringComparison.OrdinalIgnoreCase))
                    ?? allowedSortColumns.First();
            else
                sortColumn = allowedSortColumns.First();

            if (!OrderBy.IsValid(filters.SortDirection))
                sortDirection = OrderBy.Ascending;
            else
                sortDirection = filters.SortDirection!;

            var newFilters = filters with
            {
                SearchColumn = searchColumn,
                SortColumn = sortColumn,
                SortDirection = sortDirection
            };

            return newFilters;
        }
    }
}
