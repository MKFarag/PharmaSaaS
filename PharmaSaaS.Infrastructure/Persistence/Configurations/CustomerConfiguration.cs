namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

internal class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.TotalSpend)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(c => c.PhoneNumber)
            .IsUnique();

        builder.HasOne(c => c.CurrentTier)
            .WithMany()
            .HasForeignKey(c => c.CurrentTierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
