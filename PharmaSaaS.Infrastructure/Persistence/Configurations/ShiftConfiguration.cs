namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

internal class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("Shifts");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.OpeningBalance)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.ActualBalance)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.ExpectedBalance)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.Discrepancy)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.TotalSales)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.TotalReturns)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.TotalExpenses)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.Status)
            .HasConversion<int>();

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.OpenedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
