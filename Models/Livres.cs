using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliothequeManager.Models
{
    public class Livres
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Titre { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ISBN { get; set; } = string.Empty;

        public int AuteurId { get; set; }

        // Navigation
        [ForeignKey("AuteurId")]
        public Auteur? Auteur { get; set; }
        public ICollection<Exemplaire> Exemplaires { get; } = new List<Exemplaire>();
        public ICollection<LivreCategorie> LivreCategories { get; } = new List<LivreCategorie>();
        public ICollection<Reservation> Reservations { get; } = new List<Reservation>();
    }
}

