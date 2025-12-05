using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliothequeManager.Migrations
{
    /// <inheritdoc />
    public partial class AddAmandesToAdherent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateRetourReel",
                table: "Emprunts");

            migrationBuilder.AddColumn<string>(
                name: "StatutEmprunt",
                table: "Emprunts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatutEmprunt",
                table: "Emprunts");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateRetourReel",
                table: "Emprunts",
                type: "datetime2",
                nullable: true);
        }
    }
}
