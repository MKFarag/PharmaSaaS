using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmaSaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePurchaseOrderWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderSupplierPayment_PurchaseOrders_OrdersId",
                table: "PurchaseOrderSupplierPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderSupplierPayment_SupplierPayments_SupplierPaymentId",
                table: "PurchaseOrderSupplierPayment");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "SupplierPayments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<float>(
                name: "ReturnedQuantity",
                table: "PurchaseOrderItems",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "PurchaseOrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderSupplierPayment_PurchaseOrders_OrdersId",
                table: "PurchaseOrderSupplierPayment",
                column: "OrdersId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderSupplierPayment_SupplierPayments_SupplierPaymentId",
                table: "PurchaseOrderSupplierPayment",
                column: "SupplierPaymentId",
                principalTable: "SupplierPayments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderSupplierPayment_PurchaseOrders_OrdersId",
                table: "PurchaseOrderSupplierPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderSupplierPayment_SupplierPayments_SupplierPaymentId",
                table: "PurchaseOrderSupplierPayment");

            migrationBuilder.DropColumn(
                name: "ReturnedQuantity",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PurchaseOrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "SupplierPayments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderSupplierPayment_PurchaseOrders_OrdersId",
                table: "PurchaseOrderSupplierPayment",
                column: "OrdersId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderSupplierPayment_SupplierPayments_SupplierPaymentId",
                table: "PurchaseOrderSupplierPayment",
                column: "SupplierPaymentId",
                principalTable: "SupplierPayments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
