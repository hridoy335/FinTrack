using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrackCore.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddLoanTransactionTypes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "TransactionType",
            columns: new[] { "Id", "Code", "Name" },
            values: new object[,]
            {
                { 5L, "LOAN_BORROW", "Loan Borrow" },
                { 6L, "LOAN_REPAY", "Loan Repay" }
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "TransactionType",
            keyColumn: "Id",
            keyValue: 5L);

        migrationBuilder.DeleteData(
            table: "TransactionType",
            keyColumn: "Id",
            keyValue: 6L);
    }
}
