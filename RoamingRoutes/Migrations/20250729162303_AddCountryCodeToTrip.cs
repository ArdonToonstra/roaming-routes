using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoamingRoutes.Migrations
{
    /// <inheritdoc />
    public partial class AddCountryCodeToTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Trips",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Trips");
        }
    }
}
