using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BibliothequeManager.Models
{
    public class LivreAffichage
    {
        public string Titre { get; set; } = string.Empty;
        public string Auteur { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = "book.png";

        public static readonly string[] Descriptions = new string[]
        {
            "Une œuvre intemporelle sur la quête du sens, la beauté et l’essentiel.",
            "Un voyage poétique à travers les étoiles, les planètes et le cœur humain.",
            "Un classique de la dystopie qui interroge la liberté, la vérité et le pouvoir.",
            "Plongez dans un monde où la surveillance est totale et la pensée, un crime.",
            "Un récit puissant sur la mémoire, la douleur et la résilience après l’esclavage.",
            "Une exploration profonde de l’identité, de la maternité et de la justice sociale.",
            "Une aventure épique à travers la Terre du Milieu, entre amitié et destin héroïque.",
            "L’histoire d’un hobbit courageux qui change le cours du monde.",
            "Un chef-d’œuvre de la littérature française sur le temps, la mémoire et l’art.",
            "Une plongée dans les méandres de l’enfance, de l’amour et de la société.",
            "Un thriller psychologique qui brouille les frontières entre réalité et illusion.",
            "Un roman captivant où chaque chapitre cache un nouveau mystère.",
            "Une satire brillante de la société moderne, drôle et incisive.",
            "Une comédie sociale pleine d’esprit, de rebondissements et de personnages inoubliables.",
            "Un hommage poignant à la résistance de l’esprit humain face à l’adversité.",
            "Une méditation sur la solitude, l’espoir et la quête de soi.",
            "Un conte philosophique pour les grands enfants que nous sommes tous.",
            "Un classique de la science-fiction qui inspire les visions du futur depuis des décennies.",
            "Une histoire d’amour interdit dans un monde en guerre.",
            "Un récit bouleversant sur la famille, le pardon et le renouveau."
        };
    }
}
