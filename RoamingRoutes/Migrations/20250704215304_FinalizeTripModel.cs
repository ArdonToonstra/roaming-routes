using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoamingRoutes.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeTripModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BudgetCurrency",
                table: "Trips",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BudgetTotal",
                table: "Trips",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Trips",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Accommodation",
                table: "Locations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Activities",
                table: "Locations",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "Date",
                table: "Locations",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BudgetCurrency",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "BudgetTotal",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "Accommodation",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Activities",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Locations");
        }
    }
}
