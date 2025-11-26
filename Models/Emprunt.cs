using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliothequeManager.Models
{
    public class Emprunt
    {
        public int Id { get; set; }
        public int AdherentId { get; set; }
        public int ExemplaireId { get; set; }
        public int BibliothequaireEmpruntId { get; set; }
        public DateTime DateEmprunt { get; set; } = DateTime.Now;
        public DateTime DateRetourPrevu { get; set; } = DateTime.Now.AddDays(14);
        public DateTime? DateRetourReel { get; set; }
        public int? BibliothequaireRetourId { get; set; }

        // Navigation (optionnelles)
        public Adherent? Adherent { get; set; }
        public Exemplaire? Exemplaire { get; set; }
        public Bibliothecaire? BibliothequaireEmprunt { get; set; }
        public Bibliothecaire? BibliothequaireRetour { get; set; }

        public bool EstRendu() => DateRetourReel.HasValue;
        public bool EstEnRetard() => !EstRendu() && DateTime.Now > DateRetourPrevu;
    }
}
