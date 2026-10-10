namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

internal class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder
            .Property(x => x.SellingPrice)
            .HasColumnType("decimal(18,2)");

        builder
            .HasOne(x => x.Product)
            .WithOne(x => x.Inventory)
            .HasForeignKey<Inventory>(x => x.ProductId);

        builder
            .HasIndex(x => x.ProductId)
            .IsUnique();
    }
}
