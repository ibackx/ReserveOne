using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveOne.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantApiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "Tenants",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Tables",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Salons",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Restaurants",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ApiKey",
                table: "Tenants",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tables_SalonId_Nombre",
                table: "Tables",
                columns: new[] { "SalonId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Salons_RestaurantId_Nombre",
                table: "Salons",
                columns: new[] { "RestaurantId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_TenantId_Nombre",
                table: "Restaurants",
                columns: new[] { "TenantId", "Nombre" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_ApiKey",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tables_SalonId_Nombre",
                table: "Tables");

            migrationBuilder.DropIndex(
                name: "IX_Salons_RestaurantId_Nombre",
                table: "Salons");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_TenantId_Nombre",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "Tenants");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Tables",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Salons",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Restaurants",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
