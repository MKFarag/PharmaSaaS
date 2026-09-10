namespace PharmaSaaS.Application.Mapping;

public class MappingConfiguration : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        #region UserResponse

        config.NewConfig<(User user, IList<string> roles), UserResponse>()
            .Map(dest => dest, src => src.user)
            .Map(dest => dest.Roles, src => src.roles);

        #endregion

        #region ShiftResponse

        config.NewConfig<Shift, ShiftResponse>()
            .Map(dest => dest.CurrentBalance, src => src.OpeningBalance + src.TotalSales - src.TotalReturns - src.TotalExpenses);

        #endregion

        #region InvoiceMapping

        config.NewConfig<Invoice, InvoiceResponse>()
            .Map(dest => dest.CustomerName, src => src.Customer != null ? src.Customer.FullName : null);

        config.NewConfig<Invoice, InvoiceDetailResponse>()
            .Map(dest => dest.CustomerName, src => src.Customer != null ? src.Customer.FullName : null);

        config.NewConfig<InvoiceItem, InvoiceItemResponse>()
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : null)
            .Map(dest => dest.Subtotal, src => (decimal)src.Quantity * src.UnitPrice);

        config.NewConfig<Return, ReturnResponse>()
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : null);

        #endregion

        #region PurchaseOrderMapping

        config.NewConfig<PurchaseOrderItem, PurchaseOrderItemResponse>()
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : string.Empty)
            .Map(dest => dest.ReturnedQuantity, src => src.ReturnedQuantity);

        config.NewConfig<PurchaseOrder, PurchaseOrderResponse>()
            .Map(dest => dest.RemainingAmount, src => src.RemainingAmount);

        config.NewConfig<PurchaseOrder, PurchaseOrderDetailResponse>()
            .Map(dest => dest.RemainingAmount, src => src.RemainingAmount);

        #endregion

        #region SupplierPaymentMapping

        config.NewConfig<SupplierPayment, SupplierPaymentResponse>()
            .Map(dest => dest.SupplierName, src => src.Supplier != null ? src.Supplier.Name : string.Empty)
            .Map(dest => dest.OrderIds, src => src.Orders != null ? src.Orders.Select(o => o.Id).ToList() : new List<int>());

        #endregion

        #region InventoryMapping

        config.NewConfig<Inventory, InventoryResponse>()
            .Map(dest => dest.Name, src => src.Product.Name)
            .Map(dest => dest.Concentration, src => src.Product.Concentration);

        #endregion


    }
}
