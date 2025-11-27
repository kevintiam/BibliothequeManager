using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BibliothequeManager.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int AdherentId { get; set; }
        public int LivreId { get; set; }
        public int BibliothecaireId { get; set; }
        public DateTime DateReservation { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ExemplaireAttribue))]
        public int? ExemplaireAttribueId { get; set; } // Quand le livre devient disponible

        // Navigation
        public Adherent? Adherent { get; set; }
        public Livres? Livre { get; set; }
        public Bibliothecaire? Bibliothecaire { get; set; }
        public Exemplaire? ExemplaireAttribue { get; set; }
    }
}
