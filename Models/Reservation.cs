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

        public string Statut { get; set; } = "En attente";
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

        [NotMapped]
        public string StatutAffichage
        {
            get
            {
                if (EstEnRetard)
                    return "En retard";
                if (JoursRestants == 0)
                    return "À récupérer aujourd’hui";
                return $"({JoursRestants} j)";
            }
        }


        public Livres? Livre { get; set; } 

        public Adherent? Adherent { get; set; }
        public Bibliothecaire? Bibliothecaire { get; set; }
        public Exemplaire? ExemplaireAttribue { get; set; }

    }
}