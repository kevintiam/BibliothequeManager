using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [NotMapped]
        public string NomComplet => $"{Prenom} {Nom}";


        public virtual ICollection<Livres> Livres { get; } = new List<Livres>();

        [NotMapped]
        public int NombreLivres => Livres.Count;
    }
}
