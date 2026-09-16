using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PharmaSaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserRoleSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "IsDefault", "IsDisabled", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "019ea876-57f9-78e8-a72a-e0bb31c3b6a4", "019ea876-7ae1-7e71-8460-0beb2d037d38", false, false, "Admin", "ADMIN" },
                    { "019ea876-b713-7528-bbeb-003fa1d80ab5", "019ea876-cf09-7883-91b4-3ad44fee485c", true, false, "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "IsDisabled", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "019ea884-cfe2-74be-873f-2907ba32c338", 0, "019ea884-e1fc-736e-a46d-74f88d783d1c", "mkfarag-admin@PharmaSaaS.com", true, "Mohamed Khaled", false, "Farag", false, null, "MKFARAG-ADMIN@PHARMASAAS.COM", "MKFARAG-ADMIN@PHARMASAAS.COM", "AQAAAAIAAYagAAAAEO/wXsU2Tq19zlBGeTMr/ZNIjY9N40DbfywL9b4/6tHJWIh5ekuefxOjAj2v7XCCzw==", null, false, "64F1D1C1D6284264840AF3D1AF3C2CFC", false, "mkfarag-admin@PharmaSaaS.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "019ea876-57f9-78e8-a72a-e0bb31c3b6a4", "019ea884-cfe2-74be-873f-2907ba32c338" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "019ea876-57f9-78e8-a72a-e0bb31c3b6a4", "019ea884-cfe2-74be-873f-2907ba32c338" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "019ea876-57f9-78e8-a72a-e0bb31c3b6a4");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "019ea876-b713-7528-bbeb-003fa1d80ab5");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "019ea884-cfe2-74be-873f-2907ba32c338");
        }
    }
}
