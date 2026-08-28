using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinTrackCore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransactionType",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionType", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TransactionType",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1L, "INCOME", "Income" },
                    { 2L, "EXPENSE", "Expense" },
                    { 3L, "TRANSFER", "Transfer" },
                    { 4L, "OPENING_BALANCE", "Opening Balance" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionType_Code",
                table: "TransactionType",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionType");
        }
    }
}
