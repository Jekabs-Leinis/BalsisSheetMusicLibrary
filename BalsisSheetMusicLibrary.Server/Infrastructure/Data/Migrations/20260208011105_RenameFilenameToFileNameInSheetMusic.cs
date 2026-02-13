using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalsisSheetMusicLibrary.Server.Migrations
{
    /// <inheritdoc />
    public partial class RenameFilenameToFileNameInSheetMusic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Filename",
                table: "SheetMusic",
                newName: "FileName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "SheetMusic",
                newName: "Filename");
        }
    }
}
