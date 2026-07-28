using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaSaaS.Domain.Entities;

namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

public sealed class ReturnInvoiceItemConfiguration : IEntityTypeConfiguration<ReturnInvoiceItem>
{
    public void Configure(EntityTypeBuilder<ReturnInvoiceItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.RefundAmount).HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
