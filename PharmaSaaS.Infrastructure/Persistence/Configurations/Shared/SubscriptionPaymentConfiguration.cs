namespace PharmaSaaS.Infrastructure.Persistence.Configurations.Shared;

internal class SubscriptionPaymentConfiguration : IEntityTypeConfiguration<SubscriptionPayment>
{
    public void Configure(EntityTypeBuilder<SubscriptionPayment> builder)
    {
        builder.ToTable("SubscriptionPayments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PharmacyId)
            .HasMaxLength(36)
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.IsVerified)
            .HasDefaultValue(false);

        builder.Property(x => x.Amount)
            .HasPrecision(10, 2);

        builder.HasOne(x => x.Pharmacy)
            .WithMany()
            .HasForeignKey(x => x.PharmacyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
