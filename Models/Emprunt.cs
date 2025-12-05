using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace BibliothequeManager.Models
{
    public class Emprunt : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int AdherentId { get; set; }
        public int ExemplaireId { get; set; }
        public int BibliothecaireEmpruntId { get; set; }
        public DateTime DateEmprunt { get; set; } = DateTime.UtcNow;
        public DateTime DateRetourPrevu { get; set; } = DateTime.UtcNow.AddDays(14);
        public DateTime? DateRetourReel { get; set; }
        public int? BibliothecaireRetourId { get; set; }

        // Backing field pour la propriété mappée par EF Core
        private string statut = "En cours";

        public string StatutEmprunt
        {
            get => statut;
            set
            {
                if (statut != value)
                {
                    statut = value;
                    OnPropertyChanged();
                }
            }
        }

        public void MettreAJourStatut()
        {
            string statutCalcule = DateRetourReel.HasValue
                ? "Retourné"
                : DateTime.UtcNow > DateRetourPrevu
                    ? "En retard"
                    : "En cours";

            StatutEmprunt = statutCalcule; 
        }

        [NotMapped]
        public int JoursRestants
        {
            get
            {
                var now = DateTime.Today;
                var retour = DateRetourPrevu.ToLocalTime().Date;
                return (retour - now).Days;
            }
        }

        [NotMapped]
        public string Amande
        {
            get
            {
                if (DateRetourReel.HasValue || JoursRestants > 0)
                    return "0 $CAD";

                var coef = Math.Abs(JoursRestants);
                return $"{coef * 5.25m} $CAD";
            }
        }

        // Navigation
        public Adherent? Adherent { get; set; }
        public Exemplaire? Exemplaire { get; set; }
        public Bibliothecaire? BibliothecaireEmprunt { get; set; }
        public Bibliothecaire? BibliothecaireRetour { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}