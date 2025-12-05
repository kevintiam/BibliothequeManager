using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliothequeManager.Migrations
{
    /// <inheritdoc />
    public partial class chagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateRetourReel",
                table: "Emprunts",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateRetourReel",
                table: "Emprunts");
        }
    }
}
