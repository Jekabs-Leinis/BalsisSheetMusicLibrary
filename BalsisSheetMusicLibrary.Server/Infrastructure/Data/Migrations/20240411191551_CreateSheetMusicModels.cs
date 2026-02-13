using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalsisSheetMusicLibrary.Server.Migrations
{
    /// <inheritdoc />
    public partial class CreateSheetMusicModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SheetMusic",
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
                    table.PrimaryKey("PK_SheetMusic", x => x.Id);
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
                    SheetMusicId = table.Column<string>(type: "INTEGER", nullable: false),
                    Order = table.Column<uint>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetListItems", x => new { x.SetListId, x.SheetMusicId });
                    table.ForeignKey(
                        name: "FK_SetListItems_SheetMusic_SheetMusicId",
                        column: x => x.SheetMusicId,
                        principalTable: "SheetMusic",
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
                name: "IX_SetListItems_SheetMusicId",
                table: "SetListItems",
                column: "SheetMusicId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SetListItems");

            migrationBuilder.DropTable(
                name: "SheetMusic");

            migrationBuilder.DropTable(
                name: "SetLists");
        }
    }
}
