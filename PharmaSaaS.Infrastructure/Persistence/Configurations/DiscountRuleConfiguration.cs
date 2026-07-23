namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

internal class DiscountRuleConfiguration : IEntityTypeConfiguration<DiscountRule>
{
    public void Configure(EntityTypeBuilder<DiscountRule> builder)
    {
        builder.ToTable("DiscountRules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.MinInvoiceAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(r => r.DiscountPercent)
            .HasColumnType("decimal(5,2)");
    }
}
