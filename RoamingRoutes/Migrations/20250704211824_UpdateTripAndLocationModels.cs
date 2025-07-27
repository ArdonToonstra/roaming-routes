using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoamingRoutes.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTripAndLocationModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Trips",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Trips",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Day",
                table: "Locations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "Day",
                table: "Locations");
        }
    }
}
