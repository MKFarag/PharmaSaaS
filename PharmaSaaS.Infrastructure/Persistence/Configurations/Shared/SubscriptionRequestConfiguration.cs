namespace PharmaSaaS.Infrastructure.Persistence.Configurations.Shared;

internal class SubscriptionRequestConfiguration : IEntityTypeConfiguration<SubscriptionRequest>
{
    public void Configure(EntityTypeBuilder<SubscriptionRequest> builder)
    {
        builder.ToTable("SubscriptionRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PharmacyName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.OwnerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
    }
}
