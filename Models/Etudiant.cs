using System.Collections.Generic;

namespace centre_soutien.Models
{
    public class Etudiant
    {
        public int IDEtudiant { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string? DateNaissance { get; set; } // Format "YYYY-MM-DD", nullable
        public string? Adresse { get; set; }        // Nullable
        public string? Telephone { get; set; }      // Nullable, potentiellement unique
        public string? Lycee { get; set; }   
        public string NomComplet => $"{Prenom} {Nom}"; // Ou $"{Nom} {Prenom}" selon ta préférence d'affichage
        // Nullable
        public string DateInscriptionSysteme { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // Initialisé
        public string? Notes { get; set; }          // Nullable
        public bool EstArchive { get; set; }
        public string Code { get; set; } = string.Empty; // Code unique pour l'étudiant


        // Navigation: Un étudiant peut avoir plusieurs inscriptions et plusieurs paiements
        public virtual ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();
        public virtual ICollection<Paiement> Paiements { get; set; } = new List<Paiement>();
        public Etudiant()
        {
            // DateInscriptionSysteme est initialisée ci-dessus.
        }
    }
}