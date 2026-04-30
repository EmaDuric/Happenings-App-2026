using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Happenings.Services.Migrations
{
    public partial class AddMissingTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tabele Announcements i Invitations su već kreirane u FixCascadeIssue migraciji
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Ništa
        }
    }
}