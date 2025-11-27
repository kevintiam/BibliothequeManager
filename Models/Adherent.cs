using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeManager.Models
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(NumeroCarte), IsUnique = true)]
    public class Adherent
    {
        public int Id { get; set; }

        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [MaxLength(30)]
        public string Telephone { get; set; } = string.Empty;

        // Stocké en base : 10 chiffres
        [Required]
        [MaxLength(10)]
        public string NumeroCarte { get; set; } = string.Empty;

        // Affichage formaté (non mappé)
        [NotMapped]
        public string NumeroCarteFormate => Formater(NumeroCarte);

        // Formatage statique réutilisable
        public static string Formater(string numero)
        {
            if (string.IsNullOrEmpty(numero) || numero.Length != 10) return numero;
            return $"{numero[..3]}-{numero[3..6]}-{numero[6..10]}";
        }

        // Génération automatique de numéro unique (10 chiffres) via RNG cryptographique
        public static string GenererNumeroUnique()
        {
            Span<byte> bytes = stackalloc byte[8];
            RandomNumberGenerator.Fill(bytes);
            var value = BitConverter.ToUInt64(bytes);
            return (value % 10000000000UL).ToString("D10");
        }

        // Navigation
        public ICollection<Emprunt> Emprunts { get; } = new List<Emprunt>();
        public ICollection<Reservation> Reservations { get; } = new List<Reservation>();
    }
}
