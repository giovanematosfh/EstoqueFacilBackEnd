using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EstoqueFacil.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Uma migration antiga (removida do código) criou purchase_requests com outro formato
            // em alguns bancos. Renomeia em vez de apagar, para não perder dados.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.columns
                               WHERE table_schema = current_schema()
                                 AND table_name = 'purchase_requests'
                                 AND column_name = 'RequestNumber') THEN
                        ALTER TABLE purchase_requests RENAME TO purchase_requests_legacy;
                        ALTER TABLE purchase_requests_legacy RENAME CONSTRAINT "PK_purchase_requests" TO "PK_purchase_requests_legacy";
                        ALTER TABLE purchase_requests_legacy RENAME CONSTRAINT "FK_purchase_requests_branches_BranchId" TO "FK_purchase_requests_legacy_branches_BranchId";
                        ALTER TABLE purchase_requests_legacy RENAME CONSTRAINT "FK_purchase_requests_products_ProductId" TO "FK_purchase_requests_legacy_products_ProductId";
                        ALTER INDEX "IX_purchase_requests_BranchId" RENAME TO "IX_purchase_requests_legacy_BranchId";
                        ALTER INDEX "IX_purchase_requests_ProductId" RENAME TO "IX_purchase_requests_legacy_ProductId";
                    END IF;
                END $$;
                """);

            migrationBuilder.CreateTable(
                name: "purchase_requests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    BranchId = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    RequestDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReceivedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReceivedQuantity = table.Column<int>(type: "integer", nullable: true),
                    RequesterName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_purchase_requests_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_requests_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_purchase_requests_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_requests_BranchId_RequestDate",
                table: "purchase_requests",
                columns: new[] { "BranchId", "RequestDate" });

            migrationBuilder.CreateIndex(
                name: "IX_purchase_requests_CompanyId",
                table: "purchase_requests",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_requests_ProductId",
                table: "purchase_requests",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "purchase_requests");
        }
    }
}
