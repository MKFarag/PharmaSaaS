using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmaSaaS.Domain.Entities;

namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Total).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.FinalAmount).HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.Shift)
            .WithMany()
            .HasForeignKey(x => x.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
