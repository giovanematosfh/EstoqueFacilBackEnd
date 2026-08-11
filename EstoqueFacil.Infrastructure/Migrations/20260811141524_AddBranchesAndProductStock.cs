using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EstoqueFacil.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchesAndProductStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumStockQuantity",
                table: "products");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "products");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "stock_movements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "product_stocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MinimumQuantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_stocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_stocks_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_stocks_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_BranchId",
                table: "stock_movements",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_product_stocks_BranchId",
                table: "product_stocks",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_product_stocks_ProductId_BranchId",
                table: "product_stocks",
                columns: new[] { "ProductId", "BranchId" },
                unique: true);

            migrationBuilder.InsertData(
                table: "branches",
                columns: new[] { "Name", "Address", "Active", "CreatedAt" },
                values: new object[] { "Matriz", null, true, DateTime.UtcNow });

            migrationBuilder.Sql(@"
                INSERT INTO product_stocks (""ProductId"", ""BranchId"", ""Quantity"", ""MinimumQuantity"")
                SELECT p.""Id"", b.""Id"", 0, 0
                FROM products p
                CROSS JOIN branches b
                WHERE b.""Name"" = 'Matriz';
            ");

            migrationBuilder.Sql(@"
                UPDATE stock_movements
                SET ""BranchId"" = (SELECT ""Id"" FROM branches WHERE ""Name"" = 'Matriz' LIMIT 1);
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_stock_movements_branches_BranchId",
                table: "stock_movements",
                column: "BranchId",
                principalTable: "branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_stock_movements_branches_BranchId",
                table: "stock_movements");

            migrationBuilder.DropTable(
                name: "product_stocks");

            migrationBuilder.DropTable(
                name: "branches");

            migrationBuilder.DropIndex(
                name: "IX_stock_movements_BranchId",
                table: "stock_movements");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "stock_movements");

            migrationBuilder.AddColumn<int>(
                name: "MinimumStockQuantity",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
