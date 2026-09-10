using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaSaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductNameConcentrationUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Products_Name_Concentration",
                table: "Products",
                columns: new[] { "Name", "Concentration" },
                unique: true,
                filter: "[Concentration] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name_NoConcentration",
                table: "Products",
                column: "Name",
                unique: true,
                filter: "[Concentration] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Name_Concentration",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Name_NoConcentration",
                table: "Products");
        }
    }
}
