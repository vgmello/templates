using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "billing");

        migrationBuilder.CreateTable(
            name: "Cashiers",
            schema: "billing",
            columns: table => new
            {
                CashierId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                CreatedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                UpdatedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Cashiers", x => x.CashierId);
            });

        migrationBuilder.CreateTable(
            name: "Invoices",
            schema: "billing",
            columns: table => new
            {
                InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Status = table.Column<string>(type: "text", nullable: false),
                CreatedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                UpdatedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Invoices", x => x.InvoiceId);
            });

        migrationBuilder.CreateTable(
            name: "CashierCurrencies",
            schema: "billing",
            columns: table => new
            {
                CashierId = table.Column<Guid>(type: "uuid", nullable: false),
                CurrencyId = table.Column<Guid>(type: "uuid", nullable: false),
                EffectiveDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CustomCurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                CreatedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CashierCurrencies", x => new { x.CashierId, x.CurrencyId, x.EffectiveDateUtc });
                table.ForeignKey(
                    name: "FK_CashierCurrencies_Cashiers_CashierId",
                    column: x => x.CashierId,
                    principalSchema: "billing",
                    principalTable: "Cashiers",
                    principalColumn: "CashierId",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CashierCurrencies",
            schema: "billing");

        migrationBuilder.DropTable(
            name: "Invoices",
            schema: "billing");

        migrationBuilder.DropTable(
            name: "Cashiers",
            schema: "billing");
    }
}
