using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibliothequeManager.Migrations
{
    /// <inheritdoc />
    public partial class CreateDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Adherents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NumeroCarte = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adherents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Auteurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auteurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bibliothecaires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bibliothecaires", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Livres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ISBN = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AuteurId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Livres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Livres_Auteurs_AuteurId",
                        column: x => x.AuteurId,
                        principalTable: "Auteurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Exemplaires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LivreId = table.Column<int>(type: "int", nullable: false),
                    NombrePage = table.Column<int>(type: "int", nullable: false),
                    CodeBarre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EstDisponible = table.Column<bool>(type: "bit", nullable: false),
                    Etat = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exemplaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exemplaires_Livres_LivreId",
                        column: x => x.LivreId,
                        principalTable: "Livres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LivreCategories",
                columns: table => new
                {
                    LivreId = table.Column<int>(type: "int", nullable: false),
                    CategorieId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LivreCategories", x => new { x.LivreId, x.CategorieId });
                    table.ForeignKey(
                        name: "FK_LivreCategories_Categories_CategorieId",
                        column: x => x.CategorieId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LivreCategories_Livres_LivreId",
                        column: x => x.LivreId,
                        principalTable: "Livres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Emprunts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdherentId = table.Column<int>(type: "int", nullable: false),
                    ExemplaireId = table.Column<int>(type: "int", nullable: false),
                    BibliothecaireEmpruntId = table.Column<int>(type: "int", nullable: false),
                    DateEmprunt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateRetourPrevu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateRetourReel = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BibliothecaireRetourId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emprunts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Emprunts_Adherents_AdherentId",
                        column: x => x.AdherentId,
                        principalTable: "Adherents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Emprunts_Bibliothecaires_BibliothecaireEmpruntId",
                        column: x => x.BibliothecaireEmpruntId,
                        principalTable: "Bibliothecaires",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Emprunts_Bibliothecaires_BibliothecaireRetourId",
                        column: x => x.BibliothecaireRetourId,
                        principalTable: "Bibliothecaires",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Emprunts_Exemplaires_ExemplaireId",
                        column: x => x.ExemplaireId,
                        principalTable: "Exemplaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdherentId = table.Column<int>(type: "int", nullable: false),
                    LivreId = table.Column<int>(type: "int", nullable: false),
                    BibliothecaireId = table.Column<int>(type: "int", nullable: false),
                    DateReservation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExemplaireAttribueId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Adherents_AdherentId",
                        column: x => x.AdherentId,
                        principalTable: "Adherents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Bibliothecaires_BibliothecaireId",
                        column: x => x.BibliothecaireId,
                        principalTable: "Bibliothecaires",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reservations_Exemplaires_ExemplaireAttribueId",
                        column: x => x.ExemplaireAttribueId,
                        principalTable: "Exemplaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Reservations_Livres_LivreId",
                        column: x => x.LivreId,
                        principalTable: "Livres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adherents_Email",
                table: "Adherents",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Adherents_NumeroCarte",
                table: "Adherents",
                column: "NumeroCarte",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bibliothecaires_Email",
                table: "Bibliothecaires",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Nom",
                table: "Categories",
                column: "Nom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Emprunts_AdherentId",
                table: "Emprunts",
                column: "AdherentId");

            migrationBuilder.CreateIndex(
                name: "IX_Emprunts_BibliothecaireEmpruntId",
                table: "Emprunts",
                column: "BibliothecaireEmpruntId");

            migrationBuilder.CreateIndex(
                name: "IX_Emprunts_BibliothecaireRetourId",
                table: "Emprunts",
                column: "BibliothecaireRetourId");

            migrationBuilder.CreateIndex(
                name: "IX_Emprunts_ExemplaireId",
                table: "Emprunts",
                column: "ExemplaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Exemplaires_CodeBarre",
                table: "Exemplaires",
                column: "CodeBarre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exemplaires_LivreId",
                table: "Exemplaires",
                column: "LivreId");

            migrationBuilder.CreateIndex(
                name: "IX_LivreCategories_CategorieId",
                table: "LivreCategories",
                column: "CategorieId");

            migrationBuilder.CreateIndex(
                name: "IX_Livres_AuteurId",
                table: "Livres",
                column: "AuteurId");

            migrationBuilder.CreateIndex(
                name: "IX_Livres_ISBN",
                table: "Livres",
                column: "ISBN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_AdherentId",
                table: "Reservations",
                column: "AdherentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BibliothecaireId",
                table: "Reservations",
                column: "BibliothecaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ExemplaireAttribueId",
                table: "Reservations",
                column: "ExemplaireAttribueId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_LivreId",
                table: "Reservations",
                column: "LivreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Emprunts");

            migrationBuilder.DropTable(
                name: "LivreCategories");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Adherents");

            migrationBuilder.DropTable(
                name: "Bibliothecaires");

            migrationBuilder.DropTable(
                name: "Exemplaires");

            migrationBuilder.DropTable(
                name: "Livres");

            migrationBuilder.DropTable(
                name: "Auteurs");
        }
    }
}
