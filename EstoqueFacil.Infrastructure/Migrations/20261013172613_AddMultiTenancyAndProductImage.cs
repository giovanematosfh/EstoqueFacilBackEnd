using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EstoqueFacil.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancyAndProductImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_products_Sku",
                table: "products");

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "suppliers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "stock_movements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "product_stocks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "categories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "branches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MaxUsers = table.Column<int>(type: "integer", nullable: true),
                    MaxBranches = table.Column<int>(type: "integer", nullable: true),
                    MaxProducts = table.Column<int>(type: "integer", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Document = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlanId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_companies_plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Planos iniciais (editáveis no painel) e a empresa da plataforma, dona dos dados que já existiam.
            migrationBuilder.Sql("""
                INSERT INTO plans ("Name", "Price", "MaxUsers", "MaxBranches", "MaxProducts", "Active", "CreatedAt") VALUES
                    ('Ilimitado',    0,      NULL, NULL, NULL, true, now()),
                    ('Básico',       79.90,  3,    1,    500,  true, now()),
                    ('Profissional', 149.90, 10,   5,    5000, true, now()),
                    ('Empresarial',  299.90, NULL, NULL, NULL, true, now());

                INSERT INTO companies ("Name", "Active", "CreatedAt", "PlanId")
                VALUES ('Empresa principal', true, now(), (SELECT "Id" FROM plans WHERE "Name" = 'Ilimitado'));
                """);

            foreach (var table in new[] { "users", "suppliers", "stock_movements", "products", "product_stocks", "categories", "branches" })
            {
                migrationBuilder.Sql($"""UPDATE {table} SET "CompanyId" = (SELECT min("Id") FROM companies);""");
            }

            // O admin mais antigo (se houver) vira o super admin da plataforma.
            migrationBuilder.Sql("""
                INSERT INTO roles ("Name", "NormalizedName", "ConcurrencyStamp")
                SELECT 'SuperAdmin', 'SUPERADMIN', gen_random_uuid()::text
                WHERE NOT EXISTS (SELECT 1 FROM roles WHERE "NormalizedName" = 'SUPERADMIN');

                INSERT INTO user_roles ("UserId", "RoleId")
                SELECT min(ur."UserId"), (SELECT "Id" FROM roles WHERE "NormalizedName" = 'SUPERADMIN')
                FROM user_roles ur
                JOIN roles r ON r."Id" = ur."RoleId" AND r."NormalizedName" = 'ADMIN'
                HAVING min(ur."UserId") IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_users_CompanyId",
                table: "users",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_CompanyId",
                table: "suppliers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_CompanyId",
                table: "stock_movements",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_products_CompanyId_Sku",
                table: "products",
                columns: new[] { "CompanyId", "Sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_stocks_CompanyId",
                table: "product_stocks",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_categories_CompanyId",
                table: "categories",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_branches_CompanyId",
                table: "branches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_companies_PlanId",
                table: "companies",
                column: "PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_branches_companies_CompanyId",
                table: "branches",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_categories_companies_CompanyId",
                table: "categories",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_product_stocks_companies_CompanyId",
                table: "product_stocks",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_products_companies_CompanyId",
                table: "products",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_movements_companies_CompanyId",
                table: "stock_movements",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_suppliers_companies_CompanyId",
                table: "suppliers",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_users_companies_CompanyId",
                table: "users",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_branches_companies_CompanyId",
                table: "branches");

            migrationBuilder.DropForeignKey(
                name: "FK_categories_companies_CompanyId",
                table: "categories");

            migrationBuilder.DropForeignKey(
                name: "FK_product_stocks_companies_CompanyId",
                table: "product_stocks");

            migrationBuilder.DropForeignKey(
                name: "FK_products_companies_CompanyId",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_movements_companies_CompanyId",
                table: "stock_movements");

            migrationBuilder.DropForeignKey(
                name: "FK_suppliers_companies_CompanyId",
                table: "suppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_users_companies_CompanyId",
                table: "users");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "plans");

            migrationBuilder.DropIndex(
                name: "IX_users_CompanyId",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_suppliers_CompanyId",
                table: "suppliers");

            migrationBuilder.DropIndex(
                name: "IX_stock_movements_CompanyId",
                table: "stock_movements");

            migrationBuilder.DropIndex(
                name: "IX_products_CompanyId_Sku",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_product_stocks_CompanyId",
                table: "product_stocks");

            migrationBuilder.DropIndex(
                name: "IX_categories_CompanyId",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "IX_branches_CompanyId",
                table: "branches");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "stock_movements");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "products");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "product_stocks");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "branches");

            migrationBuilder.CreateIndex(
                name: "IX_products_Sku",
                table: "products",
                column: "Sku",
                unique: true);
        }
    }
}
