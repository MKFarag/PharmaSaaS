namespace PharmaSaaS.Domain.Constants;

public static class Cache
{
    public partial class Tags
    {
        public static string Roles(string tenantId) => $"{tenantId}:roles";
        public static string Users(string tenantId) => $"{tenantId}:users";
        public static string Categories(string tenantId) => $"{tenantId}:categories";
        public static string DosageForms(string tenantId) => $"{tenantId}:dosage-forms";
        public static string Customers(string tenantId) => $"{tenantId}:customers";
        public static string LoyaltyTiers(string tenantId) => $"{tenantId}:loyalty-tiers";
        public static string Suppliers(string tenantId) => $"{tenantId}:suppliers";
    }

    public partial class Keys
    {
        public static string RolesWithoutDefault(string tenantId) => $"{tenantId}:roles:without-default";
        public static string UsersWithoutDefaultRole(string tenantId) => $"{tenantId}:users:without-default-role";
        public static string AllCategories(string tenantId) => $"{tenantId}:categories:all";
        public static string AllDosageForms(string tenantId) => $"{tenantId}:dosage-forms:all";
        public static string AllCustomers(string tenantId) => $"{tenantId}:customers:all";
        public static string AllLoyaltyTiers(string tenantId) => $"{tenantId}:loyalty-tiers:all";
        public static string AllSuppliers(string tenantId) => $"{tenantId}:suppliers:all";
    }

    public static class Expirations
    {
        public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan Medium = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan Long = TimeSpan.FromHours(1);
        public static readonly TimeSpan VeryLong = TimeSpan.FromHours(24);
    }
}
