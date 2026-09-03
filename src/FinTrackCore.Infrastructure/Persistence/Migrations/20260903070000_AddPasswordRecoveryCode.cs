using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FinTrackCore.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddPasswordRecoveryCode : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "PasswordRecoveryCode",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserInfoId = table.Column<long>(type: "bigint", nullable: false),
                CodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PasswordRecoveryCode", x => x.Id);
                table.ForeignKey(
                    name: "FK_PasswordRecoveryCode_UserInfo_UserInfoId",
                    column: x => x.UserInfoId,
                    principalTable: "UserInfo",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_PasswordRecoveryCode_CodeHash",
            table: "PasswordRecoveryCode",
            column: "CodeHash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PasswordRecoveryCode_UserInfoId",
            table: "PasswordRecoveryCode",
            column: "UserInfoId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PasswordRecoveryCode");
    }
}
