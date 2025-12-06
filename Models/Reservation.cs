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
        public DateTime DateRecuperationPrevue { get; set; }

        public string Statut { get; set; } = StatutsReservation.EnAttente;
        public string Priorite { get; set; } = "Normale";

        [ForeignKey(nameof(ExemplaireAttribue))]
        public int? ExemplaireAttribueId { get; set; }


        [NotMapped]
        public int JoursRestants
        {
            get
            {
                var today = DateTime.Today;
                var prevue = DateRecuperationPrevue.Date;
                return (prevue - today).Days;
            }
        }

        [NotMapped]
        public bool EstEnRetard => JoursRestants < 0;

        public static class StatutsReservation
        {
            public const string EnAttente = "En attente";
            public const string Confirmee = "Confirmée";
            public const string EnCours = "En cours";
            public const string EnRetard = "En retard";
            public const string Annulee = "Annulée";
        }


        public Livres? Livre { get; set; } 
        public Adherent? Adherent { get; set; }
        public Bibliothecaire? Bibliothecaire { get; set; }
        public Exemplaire? ExemplaireAttribue { get; set; }
    }
}