using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliothequeManager.Models
{
    public class Adherent
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;

        // Stocké en base : 10 chiffres
        public string NumeroCarte { get; set; } = string.Empty;

        // Affichage formaté
        public string NumeroCarteFormate => Formater(NumeroCarte);

        // Formatage statique réutilisable
        public static string Formater(string numero)
        {
            if (string.IsNullOrEmpty(numero) || numero.Length != 10) return numero;
            return $"{numero[..3]}-{numero[3..6]}-{numero[6..10]}";
        }

        // Génération automatique de numéro unique (10 chiffres)
        public static string GenererNumeroUnique()
        {
            var now = DateTime.Now;
            var ticks = now.Ticks.ToString().Substring(8); // 10 derniers chiffres approximativement
            var random = new Random().Next(0, 100).ToString("D2");
            var baseNumber = (ticks + random).Substring(0, 10);

            return baseNumber;
        }
    }
}
