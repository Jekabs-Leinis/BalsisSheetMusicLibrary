using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalsisSheetMusicLibrary.Server.Migrations
{
    public partial class AddSystemFileNameToSheetMusic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SystemFileName",
                table: "SheetMusic",
                type: "TEXT",
                nullable: true);
            migrationBuilder.Sql(@"UPDATE SheetMusic SET SystemFileName = Id || '_' || Filename WHERE Filename IS NOT NULL AND Id IS NOT NULL;");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SystemFileName",
                table: "SheetMusic");
        }
    }
}
