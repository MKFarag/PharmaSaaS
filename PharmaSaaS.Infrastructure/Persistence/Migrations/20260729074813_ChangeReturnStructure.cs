using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaSaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeReturnStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReturnInvoiceItems");

            migrationBuilder.DropTable(
                name: "ReturnInvoices");

            migrationBuilder.CreateTable(
                name: "Returns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Returns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Returns_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Returns_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Returns_InvoiceId",
                table: "Returns",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_ProductId",
                table: "Returns",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Returns");

            migrationBuilder.CreateTable(
                name: "ReturnInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalInvoiceId = table.Column<int>(type: "int", nullable: false),
                    ShiftId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TotalRefund = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnInvoices_Invoices_OriginalInvoiceId",
                        column: x => x.OriginalInvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnInvoices_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReturnInvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ReturnInvoiceId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<float>(type: "real", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReturnInvoiceId1 = table.Column<int>(type: "int", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnInvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnInvoiceItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnInvoiceItems_ReturnInvoices_ReturnInvoiceId",
                        column: x => x.ReturnInvoiceId,
                        principalTable: "ReturnInvoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReturnInvoiceItems_ReturnInvoices_ReturnInvoiceId1",
                        column: x => x.ReturnInvoiceId1,
                        principalTable: "ReturnInvoices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReturnInvoiceItems_ProductId",
                table: "ReturnInvoiceItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnInvoiceItems_ReturnInvoiceId",
                table: "ReturnInvoiceItems",
                column: "ReturnInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnInvoiceItems_ReturnInvoiceId1",
                table: "ReturnInvoiceItems",
                column: "ReturnInvoiceId1");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnInvoices_OriginalInvoiceId",
                table: "ReturnInvoices",
                column: "OriginalInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnInvoices_ShiftId",
                table: "ReturnInvoices",
                column: "ShiftId");
        }
    }
}
