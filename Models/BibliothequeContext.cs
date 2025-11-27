using Microsoft.EntityFrameworkCore;
using BibliothequeManager.Models;

namespace BibliothequeManager.Models
{
    public class BibliothequeContext : DbContext
    { 
        public BibliothequeContext()
        {
        }

        public BibliothequeContext(DbContextOptions<BibliothequeContext> options)
            : base(options)
        {
        }

        public DbSet<Bibliothecaire> Bibliothecaires => Set<Bibliothecaire>();
        public DbSet<Adherent> Adherents => Set<Adherent>();
        public DbSet<Auteur> Auteurs => Set<Auteur>();
        public DbSet<Categorie> Categories => Set<Categorie>();
        public DbSet<Livres> Livres => Set<Livres>();
        public DbSet<Exemplaire> Exemplaires => Set<Exemplaire>();
        public DbSet<Emprunt> Emprunts => Set<Emprunt>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<LivreCategorie> LivreCategories => Set<LivreCategorie>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=L-107-MEDDOUR;Database=BibliothequeLiVraNova;Integrated Security=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Livres → Auteur (many-to-one)
            modelBuilder.Entity<Livres>()
                .HasOne(l => l.Auteur)
                .WithMany(a => a.Livres)
                .HasForeignKey(l => l.AuteurId)
                .OnDelete(DeleteBehavior.Restrict);

            // LivreCategorie (many-to-many)
            modelBuilder.Entity<LivreCategorie>()
                .HasKey(lc => new { lc.LivreId, lc.CategorieId });

            modelBuilder.Entity<LivreCategorie>()
                .HasOne(lc => lc.Livre)
                .WithMany(l => l.LivreCategories)
                .HasForeignKey(lc => lc.LivreId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LivreCategorie>()
                .HasOne(lc => lc.Categorie)
                .WithMany(c => c.LivreCategories)
                .HasForeignKey(lc => lc.CategorieId)
                .OnDelete(DeleteBehavior.Cascade);

            // Exemplaire → Livre (many-to-one)
            modelBuilder.Entity<Exemplaire>()
                .HasOne(e => e.Livre)
                .WithMany(l => l.Exemplaires)
                .HasForeignKey(e => e.LivreId)
                .OnDelete(DeleteBehavior.Cascade);

            // Emprunt → Adherent
            modelBuilder.Entity<Emprunt>()
                .HasOne(e => e.Adherent)
                .WithMany(a => a.Emprunts)
                .HasForeignKey(e => e.AdherentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Emprunt → Exemplaire (historique, many-to-one)
            modelBuilder.Entity<Emprunt>()
                .HasOne(e => e.Exemplaire)
                .WithMany(ex => ex.Emprunts)
                .HasForeignKey(e => e.ExemplaireId)
                .OnDelete(DeleteBehavior.Restrict);

            // Emprunt → Bibliothecaire (enregistrement)
            modelBuilder.Entity<Emprunt>()
                .HasOne(e => e.BibliothecaireEmprunt)
                .WithMany(b => b.EmpruntsEnregistres)
                .HasForeignKey(e => e.BibliothecaireEmpruntId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Emprunt → Bibliothecaire (retour)
            modelBuilder.Entity<Emprunt>()
                .HasOne(e => e.BibliothecaireRetour)
                .WithMany(b => b.EmpruntsRetournes)
                .HasForeignKey(e => e.BibliothecaireRetourId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Reservation → Adherent
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Adherent)
                .WithMany(a => a.Reservations)
                .HasForeignKey(r => r.AdherentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservation → Livre
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Livre)
                .WithMany(l => l.Reservations)
                .HasForeignKey(r => r.LivreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reservation → Bibliothecaire
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Bibliothecaire)
                .WithMany(b => b.Reservations)
                .HasForeignKey(r => r.BibliothecaireId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Reservation → ExemplaireAttribue (optionnelle)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.ExemplaireAttribue)
                .WithMany()
                .HasForeignKey(r => r.ExemplaireAttribueId)
                .OnDelete(DeleteBehavior.SetNull);

            // Contraintes et longueurs
            modelBuilder.Entity<Livres>()
                .Property(l => l.ISBN)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<Livres>()
                .HasIndex(l => l.ISBN)
                .IsUnique();

            modelBuilder.Entity<Exemplaire>()
                .Property(e => e.CodeBarre)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Exemplaire>()
                .HasIndex(e => e.CodeBarre)
                .IsUnique();

            // Adherent: longueurs (les index sont définis par attributs [Index] dans Adherent.cs)
            modelBuilder.Entity<Adherent>()
                .Property(a => a.Email)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Adherent>()
                .Property(a => a.NumeroCarte)
                .HasMaxLength(10)
                .IsRequired();

            modelBuilder.Entity<Bibliothecaire>()
                .Property(b => b.Email)
                .HasMaxLength(150)
                .IsRequired();

            // Index Email unique déjà défini par [Index] dans le modèle -> suppression de la duplication ici

            modelBuilder.Entity<Bibliothecaire>()
                .Property(b => b.PasswordHash)
                .HasMaxLength(256)
                .IsRequired();

            modelBuilder.Entity<Categorie>()
                .Property(c => c.Nom)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Categorie>()
                .HasIndex(c => c.Nom)
                .IsUnique();
        }
    }
}