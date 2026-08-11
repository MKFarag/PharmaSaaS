using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaSaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOrderSystemEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "PurchaseOrders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<float>(
                name: "ReceivedQuantity",
                table: "PurchaseOrderItems",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "ReturnReason",
                table: "PurchaseOrderItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastOrderedAt",
                table: "Inventory",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastSoldAt",
                table: "Inventory",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId2",
                table: "Inventory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "TotalUnitsSold",
                table: "Inventory",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateTable(
                name: "SupplierPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierPayments_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrderSupplierPayment",
                columns: table => new
                {
                    OrdersId = table.Column<int>(type: "int", nullable: false),
                    SupplierPaymentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrderSupplierPayment", x => new { x.OrdersId, x.SupplierPaymentId });
                    table.ForeignKey(
                        name: "FK_PurchaseOrderSupplierPayment_PurchaseOrders_OrdersId",
                        column: x => x.OrdersId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderSupplierPayment_SupplierPayments_SupplierPaymentId",
                        column: x => x.SupplierPaymentId,
                        principalTable: "SupplierPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_ProductId2",
                table: "Inventory",
                column: "ProductId2",
                unique: true,
                filter: "[ProductId2] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderSupplierPayment_SupplierPaymentId",
                table: "PurchaseOrderSupplierPayment",
                column: "SupplierPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPayments_SupplierId",
                table: "SupplierPayments",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_Products_ProductId2",
                table: "Inventory",
                column: "ProductId2",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventory_Products_ProductId2",
                table: "Inventory");

            migrationBuilder.DropTable(
                name: "PurchaseOrderSupplierPayment");

            migrationBuilder.DropTable(
                name: "SupplierPayments");

            migrationBuilder.DropIndex(
                name: "IX_Inventory_ProductId2",
                table: "Inventory");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ReceivedQuantity",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "ReturnReason",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "LastOrderedAt",
                table: "Inventory");

            migrationBuilder.DropColumn(
                name: "LastSoldAt",
                table: "Inventory");

            migrationBuilder.DropColumn(
                name: "ProductId2",
                table: "Inventory");

            migrationBuilder.DropColumn(
                name: "TotalUnitsSold",
                table: "Inventory");
        }
    }
}
