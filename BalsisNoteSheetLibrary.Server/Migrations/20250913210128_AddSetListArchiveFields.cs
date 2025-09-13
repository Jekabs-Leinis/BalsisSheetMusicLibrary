using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalsisNoteSheetLibrary.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSetListArchiveFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "SetLists",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SetLists",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "SetLists");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SetLists");
        }
    }
}
