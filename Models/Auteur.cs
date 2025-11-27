using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BibliothequeManager.Models
{
    public class Auteur
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Prenom { get; set; } = string.Empty;

        public ICollection<Livres> Livres { get; } = new List<Livres>();
    }
}
