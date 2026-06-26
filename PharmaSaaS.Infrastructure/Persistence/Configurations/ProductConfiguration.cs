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
    }
}
