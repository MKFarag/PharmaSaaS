namespace PharmaSaaS.Domain.Constants;

public static class Cache
{
    public partial class Tags
    {
        public static string Roles(string tenantId) => $"{tenantId}:roles";
        public static string Users(string tenantId) => $"{tenantId}:users";
    }

    public partial class Keys
    {
        public static string RolesWithoutDefault(string tenantId) => $"{tenantId}:roles:without-default";
        public static string UsersWithoutDefaultRole(string tenantId) => $"{tenantId}:users:without-default-role";
    }

    public static class Expirations
    {
        public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan Medium = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan Long = TimeSpan.FromHours(1);
        public static readonly TimeSpan VeryLong = TimeSpan.FromHours(24);
    }
}
