using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliothequeManager.Models
{
    public class Livres
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public int AuteurId { get; set; }

        // Navigation
        [ForeignKey("AuteurId")]
        public Auteur? Auteur { get; set; }

        public List<LivreCategorie> LivreCategories { get; } = new();
        public List<Categorie> Categories =>
            LivreCategories.Select(lc => lc.Categorie).ToList();
    }
}

