namespace PharmaSaaS.Application.Features.Roles.GetAllPermissions;

public sealed class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, IEnumerable<PermissionResponse>>
{
    private static readonly IReadOnlyList<PermissionResponse> _allPermissions = [.. Permissions
        .GetAll()
        .OfType<string>()
        .Select(permission =>
        {
            var parts = permission.Split(':', 2);
            var resource = FormatTitle(parts[0]);
            var action = parts.Length > 1 ? FormatTitle(parts[1]) : string.Empty;

            return new PermissionResponse(resource, action, permission);
        })];

    public Task<IEnumerable<PermissionResponse>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken = default)
        => Task.FromResult<IEnumerable<PermissionResponse>>(_allPermissions);

    private static string FormatTitle(string value)
    {
        if (string.Equals(value, "ai", StringComparison.OrdinalIgnoreCase))
            return "AI";

        var words = value.Split('-');
        for (var i = 0; i < words.Length; i++)
        {
            if (string.Equals(words[i], "api", StringComparison.OrdinalIgnoreCase))
                words[i] = "API";
            else if (words[i].Length > 0)
                words[i] = char.ToUpperInvariant(words[i][0]) + words[i][1..];
        }

        return string.Join(" ", words);
    }
}
