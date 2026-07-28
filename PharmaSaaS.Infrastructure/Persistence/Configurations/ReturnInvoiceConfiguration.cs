using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaSaaS.Domain.Entities;

namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

public sealed class ReturnInvoiceConfiguration : IEntityTypeConfiguration<ReturnInvoice>
{
    public void Configure(EntityTypeBuilder<ReturnInvoice> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TotalRefund).HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.OriginalInvoice)
            .WithMany(x => x.ReturnInvoices)
            .HasForeignKey(x => x.OriginalInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Shift)
            .WithMany()
            .HasForeignKey(x => x.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
