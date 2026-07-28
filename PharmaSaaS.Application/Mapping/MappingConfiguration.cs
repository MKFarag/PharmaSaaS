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

        config.NewConfig<ReturnInvoice, ReturnInvoiceSummaryResponse>();

        config.NewConfig<ReturnInvoiceItem, ReturnInvoiceItemResponse>()
            .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : null);

        #endregion
    }
}
