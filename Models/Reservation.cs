using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliothequeManager.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int AdherentId { get; set; }
        public int LivreId { get; set; }
        public int BibliothequaireId { get; set; }
        public DateTime DateReservation { get; set; } = DateTime.Now;
        public int? ExemplaireAttribueId { get; set; } // Quand le livre devient disponible

        // Navigation
        public Adherent? Adherent { get; set; }
        public Livres? Livre { get; set; }
        public Bibliothecaire? Bibliothequaire { get; set; }
    }
}
