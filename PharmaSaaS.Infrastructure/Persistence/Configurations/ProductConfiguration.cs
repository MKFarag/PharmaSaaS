namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

internal class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder
            .Property(x => x.Name)
            .HasMaxLength(200);

        builder
            .Property(x => x.Barcode)
            .HasMaxLength(50);

        builder
            .Property(x => x.Concentration)
            .HasMaxLength(50);

        builder
            .Property(x => x.MaxStorageTemperature)
            .HasColumnType("decimal(5,2)");

        builder
            .Property(x => x.StripsPerBox)
            .IsRequired(false);

        builder
            .HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.DosageForm)
            .WithMany()
            .HasForeignKey(x => x.DosageFormId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => x.Barcode)
            .IsUnique()
            .HasFilter("[Barcode] IS NOT NULL");

        // Unique (Name, Concentration) when Concentration is provided
        builder
            .HasIndex(x => new { x.Name, x.Concentration })
            .IsUnique()
            .HasFilter("[Concentration] IS NOT NULL")
            .HasDatabaseName("IX_Products_Name_Concentration");

        // Unique Name when Concentration is NULL (no concentration variant)
        builder
            .HasIndex(x => x.Name)
            .IsUnique()
            .HasFilter("[Concentration] IS NULL")
            .HasDatabaseName("IX_Products_Name_NoConcentration");
    }
}
