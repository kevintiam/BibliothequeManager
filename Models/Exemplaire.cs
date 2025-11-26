using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliothequeManager.Models
{
    public class Exemplaire
    {
        public int Id { get; set; }
        public int LivreId { get; set; }
        public int NombrePage { get; set; }
        public string CodeBarre { get; set; } = string.Empty;
        public bool EstDisponible { get; set; } = true;
        public string Etat { get; set; } = "Neuf"; // "Abîmé", "Perdu", etc.

        // Navigation
        public Livres? Livre { get; set; }
    }
}
