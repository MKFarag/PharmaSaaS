namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

internal class AIUsageLogConfiguration : IEntityTypeConfiguration<AIUsageLog>
{
    public void Configure(EntityTypeBuilder<AIUsageLog> builder)
    {
        builder
            .Property(x => x.PharmacyId)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .HasIndex(x => new { x.PharmacyId, x.UsedAt });
    }
}
