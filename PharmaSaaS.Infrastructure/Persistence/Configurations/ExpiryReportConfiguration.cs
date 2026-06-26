namespace PharmaSaaS.Infrastructure.Persistence.Configurations;

internal class ExpiryReportConfiguration : IEntityTypeConfiguration<ExpiryReport>
{
    public void Configure(EntityTypeBuilder<ExpiryReport> builder)
    {
        builder
            .Property(x => x.Notes)
            .HasMaxLength(500);

        builder
            .HasOne<Product>()
            .WithMany()
            .HasForeignKey(x => x.ProductId);

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.ReportedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
