using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Happenings.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddEventViewRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EventViews_EventId",
                table: "EventViews",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventViews_UserId",
                table: "EventViews",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventViews_Events_EventId",
                table: "EventViews",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventViews_Users_UserId",
                table: "EventViews",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventViews_Events_EventId",
                table: "EventViews");

            migrationBuilder.DropForeignKey(
                name: "FK_EventViews_Users_UserId",
                table: "EventViews");

            migrationBuilder.DropIndex(
                name: "IX_EventViews_EventId",
                table: "EventViews");

            migrationBuilder.DropIndex(
                name: "IX_EventViews_UserId",
                table: "EventViews");
        }
    }
}
