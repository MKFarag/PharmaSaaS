namespace PharmaSaaS.Infrastructure.Persistence.Configurations.Shared;

internal class PharmacyConfiguration : IEntityTypeConfiguration<Pharmacy>
{
    public void Configure(EntityTypeBuilder<Pharmacy> builder)
    {
        builder.ToTable("Pharmacies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(36).ValueGeneratedNever();
        builder.Property(x => x.TenantId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.OwnerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        builder.Property(x => x.CustomAiApiKey).HasMaxLength(200);
    }
}
