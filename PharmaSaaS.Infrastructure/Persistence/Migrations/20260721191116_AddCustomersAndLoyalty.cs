using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaSaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomersAndLoyalty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoyaltyTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SpendThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyTiers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalSpend = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentTierId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_LoyaltyTiers_CurrentTierId",
                        column: x => x.CurrentTierId,
                        principalTable: "LoyaltyTiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiscountRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoyaltyTierId = table.Column<int>(type: "int", nullable: false),
                    MinInvoiceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountRules_LoyaltyTiers_LoyaltyTierId",
                        column: x => x.LoyaltyTierId,
                        principalTable: "LoyaltyTiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CurrentTierId",
                table: "Customers",
                column: "CurrentTierId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PhoneNumber",
                table: "Customers",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRules_LoyaltyTierId",
                table: "DiscountRules",
                column: "LoyaltyTierId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTiers_Name",
                table: "LoyaltyTiers",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "DiscountRules");

            migrationBuilder.DropTable(
                name: "LoyaltyTiers");
        }
    }
}
