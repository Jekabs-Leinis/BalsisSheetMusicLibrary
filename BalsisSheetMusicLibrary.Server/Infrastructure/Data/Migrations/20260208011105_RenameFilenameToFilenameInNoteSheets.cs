using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalsisSheetMusicLibrary.Server.Migrations
{
    /// <inheritdoc />
    public partial class RenameFilenameToFilenameInNoteSheets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Filename",
                table: "NoteSheets",
                newName: "FileName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "NoteSheets",
                newName: "Filename");
        }
    }
}
