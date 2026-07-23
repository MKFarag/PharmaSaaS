namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

internal class LoyaltyTierConfiguration : IEntityTypeConfiguration<LoyaltyTier>
{
    public void Configure(EntityTypeBuilder<LoyaltyTier> builder)
    {
        builder.ToTable("LoyaltyTiers");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.SpendThreshold)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(t => t.Name)
            .IsUnique();

        builder.HasMany(t => t.DiscountRules)
            .WithOne(r => r.LoyaltyTier)
            .HasForeignKey(r => r.LoyaltyTierId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
