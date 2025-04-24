using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CurrencyConverter.Infrastructure.Migrations;

/// <inheritdoc />
public partial class UpdateExchangeRateSchema : Migration
{
    private static readonly string[] columns = new[] { "BaseCurrency", "TargetCurrency", "CreatedAt" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ExchangeRates",
            columns: table => new
            {
                ExchangeRateId = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                BaseCurrency = table.Column<string>(type: "TEXT", nullable: false),
                TargetCurrency = table.Column<string>(type: "TEXT", nullable: false),
                Rate = table.Column<decimal>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                IsHistorical = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExchangeRates", x => x.ExchangeRateId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ExchangeRates_BaseCurrency_TargetCurrency_CreatedAt",
            table: "ExchangeRates",
            columns: columns,
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ExchangeRates");
    }
}
