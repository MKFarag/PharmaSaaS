namespace PharmaSaaS.Domain.Constants;

public static class DefaultRoles
{
    public partial class Admin
    {
        public const string Name = nameof(Admin);
        public const string Id = "019ea876-57f9-78e8-a72a-e0bb31c3b6a4";
        public const string ConcurrencyStamp = "019ea876-7ae1-7e71-8460-0beb2d037d38";
    }

    public partial class User
    {
        public const string Name = nameof(User);
        public const string Id = "019ea876-b713-7528-bbeb-003fa1d80ab5";
        public const string ConcurrencyStamp = "019ea876-cf09-7883-91b4-3ad44fee485c";
    }
}
