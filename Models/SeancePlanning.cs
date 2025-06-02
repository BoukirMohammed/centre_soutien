using System;
using System.ComponentModel.DataAnnotations.Schema; // Pour [NotMapped] si besoin futur

namespace centre_soutien.Models
{
    public class SeancePlanning
    {
        public int IDSeance { get; set; } // Clé primaire pour cette définition de créneau
        
        public int IDGroupe { get; set; }
        public int IDSalle { get; set; }
        
        public DayOfWeek JourSemaine { get; set; } // Lundi, Mardi, etc. (System.DayOfWeek: Dimanche=0 à Samedi=6)
        public string HeureDebut { get; set; } = "09:00"; // Format "HH:MM"
        public string HeureFin { get; set; } = "10:00";   // Format "HH:MM"
        
        // Date à partir de laquelle ce créneau est actif (inclus)
        public string DateDebutValidite { get; set; } = DateTime.Today.ToString("yyyy-MM-dd"); 
        
        // Date jusqu'à laquelle ce créneau est actif (inclus). Si null, actif indéfiniment.
        public string? DateFinValidite { get; set; } // "YYYY-MM-DD", nullable

        public string? Notes { get; set; }
        public bool EstActif { get; set; } = true; // Pour désactiver un créneau (ex: vacances) sans le supprimer

        // Propriétés de navigation
        public virtual Groupe? Groupe { get; set; }
        public virtual Salle? Salle { get; set; }

        // Constructeur par défaut
        public SeancePlanning()
        {
            // Les valeurs par défaut sont définies au niveau des propriétés
        }
    }
}