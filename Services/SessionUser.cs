using BibliothequeManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliothequeManager.Services
{
    public class SessionUser
    {
        public Bibliothecaire? UtilisateurActuel { get; set; }
        public bool EstConnecte => UtilisateurActuel != null;

        public void SeConnecter(Bibliothecaire user)
        {
            UtilisateurActuel = user;
        }

        public void SeDeconnecter()
        {
            UtilisateurActuel = null;
        }
    }
}
