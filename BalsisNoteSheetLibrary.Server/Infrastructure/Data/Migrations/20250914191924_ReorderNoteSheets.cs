using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalsisNoteSheetLibrary.Server.Migrations
{
    /// <inheritdoc />
    public partial class ReorderNoteSheets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create a new table with the desired column order
            migrationBuilder.CreateTable(
                name: "NoteSheets_temp",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Author = table.Column<string>(type: "TEXT", nullable: true),
                    Lyricist = table.Column<string>(type: "TEXT", nullable: true),
                    Year = table.Column<uint>(type: "INTEGER", nullable: true),
                    Filename = table.Column<string>(type: "TEXT", nullable: true),
                    SystemFileName = table.Column<string>(type: "TEXT", nullable: true),
                    IsLatvian = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteSheets", x => x.Id);
                });

            // Copy data from old table to new table
            migrationBuilder.Sql(@"
                INSERT INTO NoteSheets_temp (Id, Title, Author, Lyricist, Year, Filename, SystemFileName, IsLatvian)
                SELECT Id, Title, Author, Lyricist, Year, Filename, SystemFileName, IsLatvian FROM NoteSheets;");

            // Drop the old table
            migrationBuilder.DropTable(name: "NoteSheets");

            // Rename the new table
            migrationBuilder.RenameTable(
                name: "NoteSheets_temp",
                newName: "NoteSheets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to the original column order if needed
            migrationBuilder.CreateTable(
                name: "NoteSheets_temp",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Author = table.Column<string>(type: "TEXT", nullable: true),
                    Filename = table.Column<string>(type: "TEXT", nullable: true),
                    IsLatvian = table.Column<bool>(type: "INTEGER", nullable: false),
                    Lyricist = table.Column<string>(type: "TEXT", nullable: true),
                    SystemFileName = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Year = table.Column<uint>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteSheets", x => x.Id);
                });

            // Copy data back to the original order
            migrationBuilder.Sql(@"
                INSERT INTO NoteSheets_temp (Id, Title, Author, Lyricist, Year, Filename, SystemFileName, IsLatvian)
                SELECT Id, Title, Author, Lyricist, Year, Filename, SystemFileName, IsLatvian FROM NoteSheets;");

            // Drop the table with the new order
            migrationBuilder.DropTable(name: "NoteSheets");

            // Rename the temp table back to the original name
            migrationBuilder.RenameTable(
                name: "NoteSheets_temp",
                newName: "NoteSheets");
        }
    }
}
