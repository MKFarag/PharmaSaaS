namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

internal class DosageFormConfiguration : IEntityTypeConfiguration<DosageForm>
{
    public void Configure(EntityTypeBuilder<DosageForm> builder)
    {
        builder
            .Property(x => x.Name)
            .HasMaxLength(100);

        builder
            .HasIndex(x => x.Name)
            .IsUnique();
    }
}
