using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BibliothequeManager.Models
{
    public class Exemplaire
    {
        public int Id { get; set; }
        public int LivreId { get; set; }

        [Range(1, int.MaxValue)]
        public int NombrePage { get; set; }

        [Required]
        [MaxLength(50)]
        public string CodeBarre { get; set; } = string.Empty;

        public bool EstDisponible { get; set; } = true;

        [MaxLength(30)]
        public string Etat { get; set; } = "Neuf"; // "Abîmé", "Perdu", etc.

        // Navigation
        public Livres? Livre { get; set; }
        public ICollection<Emprunt> Emprunts { get; } = new List<Emprunt>();
    }
}
