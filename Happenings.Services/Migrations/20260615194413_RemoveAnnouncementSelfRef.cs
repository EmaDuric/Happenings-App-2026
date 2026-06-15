using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Happenings.Services.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAnnouncementSelfRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Announcements_AnnouncementId",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_AnnouncementId",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "AnnouncementId",
                table: "Announcements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnnouncementId",
                table: "Announcements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_AnnouncementId",
                table: "Announcements",
                column: "AnnouncementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Announcements_AnnouncementId",
                table: "Announcements",
                column: "AnnouncementId",
                principalTable: "Announcements",
                principalColumn: "Id");
        }
    }
}
