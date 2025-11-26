using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliothequeManager.Models
{
    public class LivreCategorie
    {
        public int LivreId { get; set; }
        public Livres Livre { get; set; } = null!;

        public int CategorieId { get; set; }
        public Categorie Categorie { get; set; } = null!;
    }
}
