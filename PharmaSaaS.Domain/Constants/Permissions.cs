namespace PharmaSaaS.Domain.Constants;

public static class Permissions
{
    public static string Type { get; } = nameof(Permissions).ToLower();

    #region Role

    public const string ReadRole = "role:read";
    public const string AddRole = "role:add";
    public const string UpdateRole = "role:modify";
    public const string ToggleRoleStatus = "role:toggle-status";

    #endregion

    #region User

    public const string ReadUser = "user:read";
    public const string AddUser = "user:add";
    public const string UpdateUser = "user:modify";
    public const string ToggleUserStatus = "user:toggle-status";

    #endregion

    public static IList<string?> GetAll()
        => [.. typeof(Permissions).GetFields().Select(x => x.GetValue(x) as string)];
}
