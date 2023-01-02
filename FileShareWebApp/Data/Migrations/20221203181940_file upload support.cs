using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileShareWebApp.Data.Migrations
{
    public partial class fileuploadsupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "FileContent",
                table: "FileModel",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<bool>(
                name: "FileStoredInDb",
                table: "FileModel",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContent",
                table: "FileModel");

            migrationBuilder.DropColumn(
                name: "FileStoredInDb",
                table: "FileModel");
        }
    }
}
