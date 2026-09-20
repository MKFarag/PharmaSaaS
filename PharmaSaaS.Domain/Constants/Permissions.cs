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

    #region Inventory

    public const string ReadInventory = "inventory:read";
    public const string UpdateInventory = "inventory:modify";
    public const string ToggleRare = "inventory:toggle-rare";
    public const string AdjustInventoryStock = "inventory:adjust-stock";

    #endregion

    #region ExpiryReport

    public const string ReadExpiryReport = "expiry-report:read";
    public const string LogExpiryReport = "expiry-report:log";
    public const string RevertExpiryReport = "expiry-report:revert";

    #endregion

    #region Customer

    public const string ReadCustomer = "customer:read";
    public const string AddCustomer = "customer:add";
    public const string UpdateCustomer = "customer:modify";

    #endregion

    #region Loyalty

    public const string ReadLoyalty = "loyalty:read";
    public const string ManageLoyalty = "loyalty:manage";

    #endregion

    #region Shift

    public const string ReadShift = "shift:read";
    public const string OpenShift = "shift:open";
    public const string CloseShift = "shift:close";

    #endregion

    #region Invoice

    public const string ReadInvoice = "invoice:read";
    public const string CreateInvoice = "invoice:create";
    public const string ProcessReturn = "invoice:return";
    public const string PreviewReturn = "invoice:return-preview";

    #endregion

    #region Expense

    public const string ReadExpense = "expense:read";
    public const string LogExpense = "expense:log";
    public const string DeleteExpense = "expense:delete";

    #endregion

    #region Supplier

    public const string ReadSupplier = "supplier:read";
    public const string AddSupplier = "supplier:add";
    public const string UpdateSupplier = "supplier:modify";
    public const string DeleteSupplier = "supplier:delete";
    public const string ResetSupplierBalance = "supplier:reset-balance";

    #endregion

    #region PurchaseOrder

    public const string ReadPurchaseOrder = "purchase-order:read";
    public const string ManagePurchaseOrder = "purchase-order:manage";

    #endregion

    #region Reports

    public const string ReadDailyReport = "report:daily";
    public const string ReadProfitReport = "report:profit";
    public const string ReadShortageReport = "report:shortage";
    public const string ReadExpenseReport = "report:expenses";
    public const string ReadTopProductsReport = "report:top-products";

    #endregion

    public static IList<string?> GetAll()
        => [.. typeof(Permissions).GetFields().Select(x => x.GetValue(x) as string)];
}
