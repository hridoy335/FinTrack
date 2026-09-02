using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinTrackCore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionAndVoucherLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserInfoId = table.Column<long>(type: "bigint", nullable: false),
                    FinancialYearId = table.Column<long>(type: "bigint", nullable: false),
                    TransactionTypeId = table.Column<long>(type: "bigint", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaction_FinancialYear_FinancialYearId",
                        column: x => x.FinancialYearId,
                        principalTable: "FinancialYear",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transaction_TransactionType_TransactionTypeId",
                        column: x => x.TransactionTypeId,
                        principalTable: "TransactionType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transaction_UserInfo_UserInfoId",
                        column: x => x.UserInfoId,
                        principalTable: "UserInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VoucherLine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransactionId = table.Column<long>(type: "bigint", nullable: false),
                    CoaId = table.Column<long>(type: "bigint", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    DebitAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoucherLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoucherLine_Coa_CoaId",
                        column: x => x.CoaId,
                        principalTable: "Coa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VoucherLine_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_FinancialYearId",
                table: "Transaction",
                column: "FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_TransactionTypeId",
                table: "Transaction",
                column: "TransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_UserInfoId",
                table: "Transaction",
                column: "UserInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_UserInfoId_TransactionDate",
                table: "Transaction",
                columns: new[] { "UserInfoId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_VoucherLine_CoaId",
                table: "VoucherLine",
                column: "CoaId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherLine_TransactionId",
                table: "VoucherLine",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_VoucherLine_TransactionId_LineNumber",
                table: "VoucherLine",
                columns: new[] { "TransactionId", "LineNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VoucherLine");

            migrationBuilder.DropTable(
                name: "Transaction");
        }
    }
}
