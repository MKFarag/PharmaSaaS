namespace PharmaSaaS.Domain.Abstraction;

/// <summary>Represents a paginated list of items of type <typeparamref name="T"/></summary>
/// <typeparam name="T">The type of elements in the list</typeparam>
public interface IPaginatedList<T> where T : class
{
    List<T> Items { get; }
    int PageNumber { get; }
    int TotalPages { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}

/// <summary>Factory for creating empty paginated lists</summary>
public static class EmptyPaginatedList
{
    private class Empty<T> : IPaginatedList<T> where T : class
    {
        public List<T> Items { get; } = [];
        public int PageNumber { get; } = 1;
        public int TotalPages { get; } = 0;
        public bool HasPreviousPage => false;
        public bool HasNextPage => false;
    }

    public static IPaginatedList<T> Create<T>() where T : class
        => new Empty<T>();
}