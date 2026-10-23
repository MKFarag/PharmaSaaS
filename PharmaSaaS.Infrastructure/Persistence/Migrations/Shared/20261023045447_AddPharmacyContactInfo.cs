using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaSaaS.Infrastructure.Persistence.Migrations.Shared
{
    /// <inheritdoc />
    public partial class AddPharmacyContactInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Pharmacies",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "Pharmacies",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Pharmacies",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "Pharmacies");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Pharmacies");
        }
    }
}
