using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinTrackCore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountTypeAndCoa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountType",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalBalance = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Coa",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserInfoId = table.Column<long>(type: "bigint", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    AccountTypeId = table.Column<long>(type: "bigint", nullable: false),
                    AccountCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsSystemDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coa_AccountType_AccountTypeId",
                        column: x => x.AccountTypeId,
                        principalTable: "AccountType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Coa_Coa_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Coa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Coa_UserInfo_UserInfoId",
                        column: x => x.UserInfoId,
                        principalTable: "UserInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AccountType",
                columns: new[] { "Id", "Code", "Name", "NormalBalance" },
                values: new object[,]
                {
                    { 1L, "ASSET", "Asset", "DEBIT" },
                    { 2L, "LIABILITY", "Liability", "CREDIT" },
                    { 3L, "EQUITY", "Equity", "CREDIT" },
                    { 4L, "INCOME", "Income", "CREDIT" },
                    { 5L, "EXPENSE", "Expense", "DEBIT" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountType_Code",
                table: "AccountType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coa_AccountTypeId",
                table: "Coa",
                column: "AccountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Coa_ParentId",
                table: "Coa",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Coa_UserInfoId",
                table: "Coa",
                column: "UserInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Coa_UserInfoId_AccountCode",
                table: "Coa",
                columns: new[] { "UserInfoId", "AccountCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Coa");

            migrationBuilder.DropTable(
                name: "AccountType");
        }
    }
}
