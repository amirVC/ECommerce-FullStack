using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicIdToProductsAndCategories : Migration
    {
        /// <inheritdoc />

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string[] tables =
            {
        "Users",
        "Shipments",
        "Reviews",
        "RefundRequests",
        "Products",
        "Payments",
        "Orders",
        "DeliveryMethods",
        "Coupons",
        "Categories",
        "Campaigns",
        "Addresses"
    };

            // 1. Add nullable columns first so existing rows are preserved.
            foreach (var table in tables)
            {
                migrationBuilder.AddColumn<Guid>(
                    name: "PublicId",
                    table: table,
                    type: "uuid",
                    nullable: true);
            }

            // 2. Generate a distinct UUID for every existing row.
            foreach (var table in tables)
            {
                migrationBuilder.Sql($@"
            UPDATE ""{table}""
            SET ""PublicId"" = gen_random_uuid()
            WHERE ""PublicId"" IS NULL;
        ");
            }

            // 3. Make the columns required and create unique indexes.
            foreach (var table in tables)
            {
                migrationBuilder.AlterColumn<Guid>(
                    name: "PublicId",
                    table: table,
                    type: "uuid",
                    nullable: false,
                    oldClrType: typeof(Guid),
                    oldType: "uuid",
                    oldNullable: true);

                migrationBuilder.CreateIndex(
                    name: $"IX_{table}_PublicId",
                    table: table,
                    column: "PublicId",
                    unique: true);
            }

            // 4. Preserve the fixed IDs for seeded delivery methods.
            migrationBuilder.UpdateData(
                table: "DeliveryMethods",
                keyColumn: "Id",
                keyValue: 1,
                column: "PublicId",
                value: new Guid("7f9c8e31-4a72-4b1f-8c36-1a6e9b7d2401"));

            migrationBuilder.UpdateData(
                table: "DeliveryMethods",
                keyColumn: "Id",
                keyValue: 2,
                column: "PublicId",
                value: new Guid("c4e2a951-8d63-47b9-a215-6f3c8b742e10"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_PublicId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_PublicId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_PublicId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_RefundRequests_PublicId",
                table: "RefundRequests");

            migrationBuilder.DropIndex(
                name: "IX_Products_PublicId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PublicId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PublicId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryMethods_PublicId",
                table: "DeliveryMethods");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_PublicId",
                table: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Categories_PublicId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_PublicId",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_PublicId",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "RefundRequests");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "DeliveryMethods");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Addresses");
        }
    }
}
