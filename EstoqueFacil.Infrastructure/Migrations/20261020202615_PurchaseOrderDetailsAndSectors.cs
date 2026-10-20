using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EstoqueFacil.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseOrderDetailsAndSectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpectedDeliveryDate",
                table: "purchase_requests",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTerms",
                table: "purchase_requests",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SectorId",
                table: "purchase_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "purchase_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "purchase_request_items",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "sectors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ResponsibleName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sectors_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_requests_SectorId",
                table: "purchase_requests",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_requests_SupplierId",
                table: "purchase_requests",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_sectors_CompanyId_Name",
                table: "sectors",
                columns: new[] { "CompanyId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_requests_sectors_SectorId",
                table: "purchase_requests",
                column: "SectorId",
                principalTable: "sectors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_purchase_requests_suppliers_SupplierId",
                table: "purchase_requests",
                column: "SupplierId",
                principalTable: "suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_purchase_requests_sectors_SectorId",
                table: "purchase_requests");

            migrationBuilder.DropForeignKey(
                name: "FK_purchase_requests_suppliers_SupplierId",
                table: "purchase_requests");

            migrationBuilder.DropTable(
                name: "sectors");

            migrationBuilder.DropIndex(
                name: "IX_purchase_requests_SectorId",
                table: "purchase_requests");

            migrationBuilder.DropIndex(
                name: "IX_purchase_requests_SupplierId",
                table: "purchase_requests");

            migrationBuilder.DropColumn(
                name: "ExpectedDeliveryDate",
                table: "purchase_requests");

            migrationBuilder.DropColumn(
                name: "PaymentTerms",
                table: "purchase_requests");

            migrationBuilder.DropColumn(
                name: "SectorId",
                table: "purchase_requests");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "purchase_requests");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "purchase_request_items");
        }
    }
}
