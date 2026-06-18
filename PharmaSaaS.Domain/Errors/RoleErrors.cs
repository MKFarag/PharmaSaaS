namespace PharmaSaaS.Domain.Errors;

public record RoleErrors
{
    public static readonly Error NotFound =
        new("Role.NotFound", "No role found", StatusCodes.NotFound);

    public static readonly Error DuplicatedName =
        new("Role.DuplicatedName", "Another role with the same name is already exists", StatusCodes.Conflict);

    public static readonly Error InvalidPermissions =
        new("Role.InvalidPermissions", "Invalid permissions", StatusCodes.BadRequest);

    public static readonly Error CreationCanceled =
        new("Role.CreationCanceled", "Role creation was canceled", StatusCodes.RequestCanceled);
}
