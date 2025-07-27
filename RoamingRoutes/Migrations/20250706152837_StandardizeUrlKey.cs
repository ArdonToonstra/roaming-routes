using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoamingRoutes.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeUrlKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UrlKey",
                table: "Trips",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CityGuides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UrlKey = table.Column<string>(type: "TEXT", nullable: false),
                    CityName = table.Column<string>(type: "TEXT", nullable: false),
                    Country = table.Column<string>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", nullable: false),
                    Introduction = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityGuides", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuideSection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Icon = table.Column<string>(type: "TEXT", nullable: false),
                    CityGuideId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuideSection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuideSection_CityGuides_CityGuideId",
                        column: x => x.CityGuideId,
                        principalTable: "CityGuides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Highlight",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Costs = table.Column<string>(type: "TEXT", nullable: true),
                    References = table.Column<string>(type: "TEXT", nullable: true),
                    GuideSectionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Highlight", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Highlight_GuideSection_GuideSectionId",
                        column: x => x.GuideSectionId,
                        principalTable: "GuideSection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuideSection_CityGuideId",
                table: "GuideSection",
                column: "CityGuideId");

            migrationBuilder.CreateIndex(
                name: "IX_Highlight_GuideSectionId",
                table: "Highlight",
                column: "GuideSectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Highlight");

            migrationBuilder.DropTable(
                name: "GuideSection");

            migrationBuilder.DropTable(
                name: "CityGuides");

            migrationBuilder.DropColumn(
                name: "UrlKey",
                table: "Trips");
        }
    }
}
