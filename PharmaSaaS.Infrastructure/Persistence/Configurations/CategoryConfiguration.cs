namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder
            .Property(x => x.Name)
            .HasMaxLength(100);

        builder
            .HasIndex(x => x.Name)
            .IsUnique();
    }
}
