using Microsoft.EntityFrameworkCore.Migrations;

namespace GCPTestAPI.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PointValues",
                columns: table => new
                {
                    Real = table.Column<double>(type: "float", nullable: false),
                    Imaginary = table.Column<double>(type: "float", nullable: false),
                    Iterations = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointValues", x => new { x.Real, x.Imaginary });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PointValues");
        }
    }
}
