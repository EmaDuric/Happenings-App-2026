using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Happenings.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToOrganizer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOrganizer",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Organizers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Organizers_UserId",
                table: "Organizers",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Organizers_Users_UserId",
                table: "Organizers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizers_Users_UserId",
                table: "Organizers");

            migrationBuilder.DropIndex(
                name: "IX_Organizers_UserId",
                table: "Organizers");

            migrationBuilder.DropColumn(
                name: "IsOrganizer",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Organizers");
        }
    }
}
