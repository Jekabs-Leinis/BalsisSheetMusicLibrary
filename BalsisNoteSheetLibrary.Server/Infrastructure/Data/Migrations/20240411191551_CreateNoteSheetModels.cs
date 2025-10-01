using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalsisNoteSheetLibrary.Server.Migrations
{
    /// <inheritdoc />
    public partial class CreateNoteSheetModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NoteSheets",
                columns: table => new
                {
                    Id = table.Column<string>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Author = table.Column<string>(type: "TEXT", nullable: true),
                    Lyricist = table.Column<string>(type: "TEXT", nullable: true),
                    Year = table.Column<int>(type: "INTEGER", nullable: true),
                    Filename = table.Column<string>(type: "TEXT", nullable: true),
                    IsLatvian = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteSheets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SetLists",
                columns: table => new
                {
                    Id = table.Column<string>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Order = table.Column<uint>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SetListItems",
                columns: table => new
                {
                    SetListId = table.Column<string>(type: "INTEGER", nullable: false),
                    NoteSheetId = table.Column<string>(type: "INTEGER", nullable: false),
                    Order = table.Column<uint>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetListItems", x => new { x.SetListId, x.NoteSheetId });
                    table.ForeignKey(
                        name: "FK_SetListItems_NoteSheets_NoteSheetId",
                        column: x => x.NoteSheetId,
                        principalTable: "NoteSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SetListItems_SetLists_SetListId",
                        column: x => x.SetListId,
                        principalTable: "SetLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SetListItems_NoteSheetId",
                table: "SetListItems",
                column: "NoteSheetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SetListItems");

            migrationBuilder.DropTable(
                name: "NoteSheets");

            migrationBuilder.DropTable(
                name: "SetLists");
        }
    }
}
