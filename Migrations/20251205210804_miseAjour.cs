using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliothequeManager.Migrations
{
    /// <inheritdoc />
    public partial class miseAjour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateRetour",
                table: "Reservations",
                newName: "DateRecuperationPrevue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateRecuperationPrevue",
                table: "Reservations",
                newName: "DateRetour");
        }
    }
}
