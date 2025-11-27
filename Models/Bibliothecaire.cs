using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BibliothequeManager.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Bibliothecaire
    {
        public int Id { get; set; }

        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        // Navigation
        public ICollection<Emprunt> EmpruntsEnregistres { get; } = new List<Emprunt>();
        public ICollection<Emprunt> EmpruntsRetournes { get; } = new List<Emprunt>();
        public ICollection<Reservation> Reservations { get; } = new List<Reservation>();
    }
}
