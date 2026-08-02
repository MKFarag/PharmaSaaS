namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

internal sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.ContactInfo)
            .HasMaxLength(500);

        builder.Property(s => s.Balance)
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.HasIndex(s => s.Name)
            .IsUnique();
    }
}
