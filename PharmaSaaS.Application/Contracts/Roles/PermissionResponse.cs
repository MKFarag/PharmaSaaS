namespace PharmaSaaS.Application.Contracts.Roles;

public record PermissionResponse(
    string Resource,
    string Action,
    string Permission
);
