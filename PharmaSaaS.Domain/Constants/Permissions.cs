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

    #region Category

    public const string ReadCategory = "category:read";
    public const string AddCategory = "category:add";
    public const string UpdateCategory = "category:modify";
    public const string DeleteCategory = "category:delete";

    #endregion

    #region DosageForm

    public const string ReadDosageForm = "dosage-form:read";
    public const string AddDosageForm = "dosage-form:add";
    public const string UpdateDosageForm = "dosage-form:modify";
    public const string DeleteDosageForm = "dosage-form:delete";

    #endregion

    #region Product

    public const string ReadProduct = "product:read";
    public const string AddProduct = "product:add";
    public const string UpdateProduct = "product:modify";
    public const string DeleteProduct = "product:delete";

    #endregion

    public static IList<string?> GetAll()
        => [.. typeof(Permissions).GetFields().Select(x => x.GetValue(x) as string)];
}
