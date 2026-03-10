using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Happenings.Services.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTicketType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventTicketTypeId",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EventTicketType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTicketType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventTicketType_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_EventTicketTypeId",
                table: "Reservations",
                column: "EventTicketTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EventTicketType_EventId",
                table: "EventTicketType",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_EventTicketType_EventTicketTypeId",
                table: "Reservations",
                column: "EventTicketTypeId",
                principalTable: "EventTicketType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_EventTicketType_EventTicketTypeId",
                table: "Reservations");

            migrationBuilder.DropTable(
                name: "EventTicketType");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_EventTicketTypeId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "EventTicketTypeId",
                table: "Reservations");
        }
    }
}
