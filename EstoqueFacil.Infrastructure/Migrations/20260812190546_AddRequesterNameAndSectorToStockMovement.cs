using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstoqueFacil.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequesterNameAndSectorToStockMovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequesterName",
                table: "stock_movements",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sector",
                table: "stock_movements",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequesterName",
                table: "stock_movements");

            migrationBuilder.DropColumn(
                name: "Sector",
                table: "stock_movements");
        }
    }
}
