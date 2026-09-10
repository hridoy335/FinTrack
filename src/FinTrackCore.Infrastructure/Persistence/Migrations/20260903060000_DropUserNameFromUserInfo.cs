using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrackCore.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class DropUserNameFromUserInfo : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_UserInfo_UserName",
            table: "UserInfo");

        migrationBuilder.DropColumn(
            name: "UserName",
            table: "UserInfo");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "UserName",
            table: "UserInfo",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "IX_UserInfo_UserName",
            table: "UserInfo",
            column: "UserName",
            unique: true);
    }
}
