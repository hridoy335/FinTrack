using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrackCore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleAuthToUserInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "UserInfo",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "GoogleSubject",
                table: "UserInfo",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserInfo_GoogleSubject",
                table: "UserInfo",
                column: "GoogleSubject",
                unique: true,
                filter: "\"GoogleSubject\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserInfo_GoogleSubject",
                table: "UserInfo");

            migrationBuilder.DropColumn(
                name: "GoogleSubject",
                table: "UserInfo");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "UserInfo",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
