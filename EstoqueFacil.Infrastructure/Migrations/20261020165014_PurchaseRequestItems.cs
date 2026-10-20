using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EstoqueFacil.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseRequestItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "purchase_request_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    PurchaseRequestId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReceivedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReceivedQuantity = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_request_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchase_request_items_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_request_items_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_purchase_request_items_purchase_requests_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalTable: "purchase_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Cada SC antiga (um produto por linha) vira item. Linhas com o mesmo nº na mesma
            // filial passam a ser uma única SC, com um item por produto.
            migrationBuilder.Sql("""
                INSERT INTO purchase_request_items
                    ("CompanyId", "PurchaseRequestId", "ProductId", "Quantity", "Status", "ReceivedDate", "ReceivedQuantity")
                SELECT pr."CompanyId", h."HeaderId", pr."ProductId",
                       SUM(pr."Quantity"), MIN(pr."Status"), MAX(pr."ReceivedDate"),
                       CASE WHEN COUNT(pr."ReceivedQuantity") = 0 THEN NULL ELSE SUM(pr."ReceivedQuantity") END
                FROM purchase_requests pr
                JOIN (
                    SELECT "CompanyId", "BranchId", "Number", MIN("Id") AS "HeaderId"
                    FROM purchase_requests
                    GROUP BY "CompanyId", "BranchId", "Number"
                ) h ON h."CompanyId" = pr."CompanyId" AND h."BranchId" = pr."BranchId" AND h."Number" = pr."Number"
                GROUP BY pr."CompanyId", h."HeaderId", pr."ProductId";

                DELETE FROM purchase_requests pr
                WHERE NOT EXISTS (SELECT 1 FROM purchase_request_items i WHERE i."PurchaseRequestId" = pr."Id");

                UPDATE purchase_requests pr SET "Status" = CASE
                    WHEN EXISTS (SELECT 1 FROM purchase_request_items i WHERE i."PurchaseRequestId" = pr."Id" AND i."Status" = 1) THEN 1
                    WHEN EXISTS (SELECT 1 FROM purchase_request_items i WHERE i."PurchaseRequestId" = pr."Id" AND i."Status" = 2) THEN 2
                    ELSE 3 END;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_purchase_requests_products_ProductId",
                table: "purchase_requests");

            migrationBuilder.DropIndex(
                name: "IX_purchase_requests_CompanyId",
                table: "purchase_requests");

            migrationBuilder.DropIndex(
                name: "IX_purchase_requests_ProductId",
                table: "purchase_requests");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "purchase_requests");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "purchase_requests");

            migrationBuilder.DropColumn(
                name: "ReceivedDate",
                table: "purchase_requests");

            migrationBuilder.DropColumn(
                name: "ReceivedQuantity",
                table: "purchase_requests");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_requests_CompanyId_BranchId_Number",
                table: "purchase_requests",
                columns: new[] { "CompanyId", "BranchId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_request_items_CompanyId",
                table: "purchase_request_items",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_request_items_ProductId",
                table: "purchase_request_items",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_request_items_PurchaseRequestId_ProductId",
                table: "purchase_request_items",
                columns: new[] { "PurchaseRequestId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "purchase_request_items");

            migrationBuilder.DropIndex(
                name: "IX_purchase_requests_CompanyId_BranchId_Number",
                table: "purchase_requests");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "purchase_requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "purchase_requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ReceivedDate",
                table: "purchase_requests",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceivedQuantity",
                table: "purchase_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_requests_CompanyId",
                table: "purchase_requests",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_requests_ProductId",
                table: "purchase_requests",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_requests_products_ProductId",
                table: "purchase_requests",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
