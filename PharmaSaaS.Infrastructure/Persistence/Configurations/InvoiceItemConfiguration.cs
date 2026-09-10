namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

public sealed class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Ignore(x => x.Subtotal);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // InvoiceId cascade will be configured in DbContext globally to override restricted deletes
    }
}
