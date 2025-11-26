using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliothequeManager.Models
{
    public class BibliothequeContext : DbContext
    {
        public DbSet<Bibliothecaire> Bibliothecaires => Set<Bibliothecaire>();
        public DbSet<Adherent> Adherents => Set<Adherent>();
        public DbSet<Auteur> Auteurs => Set<Auteur>();
        public DbSet<Categorie> Categories => Set<Categorie>();
        public DbSet<Livres> Livres => Set<Livres>();
        public DbSet<Exemplaire> Exemplaires => Set<Exemplaire>();
        public DbSet<Emprunt> Emprunts => Set<Emprunt>();
        public DbSet<Reservation> Reservations => Set<Reservation>();

        
    }
}
