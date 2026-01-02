using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveOne.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VersionedRestaurantConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaxSeats",
                table: "RestaurantConfigs",
                newName: "MaxSeatsOverride");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RestaurantConfigs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "RestaurantConfigs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "RestaurantConfigs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "RestaurantConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantConfigs_RestaurantId_IsActive",
                table: "RestaurantConfigs",
                columns: new[] { "RestaurantId", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RestaurantConfigs_RestaurantId_IsActive",
                table: "RestaurantConfigs");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RestaurantConfigs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "RestaurantConfigs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "RestaurantConfigs");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "RestaurantConfigs");

            migrationBuilder.RenameColumn(
                name: "MaxSeatsOverride",
                table: "RestaurantConfigs",
                newName: "MaxSeats");
        }
    }
}
