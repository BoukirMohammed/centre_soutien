using System.Collections.Generic;

namespace centre_soutien.Models
{
    public class Groupe
    {
        public int IDGroupe { get; set; }
        public string NomDescriptifGroupe { get; set; }= string.Empty;
        public int IDMatiere { get; set; }
        public int IDProfesseur { get; set; }
        public string? Niveau { get; set; }     // Nullable
        public string? Notes { get; set; } 
        public bool EstArchive { get; set; }

        // Navigation vers les entités "parentes" et "enfants"
        // Propriétés de navigation
        public virtual Matiere? Matiere { get; set; } // Peut être null si non chargé explicitement
        public virtual Professeur? Professeur { get; set; } // Peut être null si non chargé explicitement
        
        public virtual ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();
        public virtual ICollection<SeancePlanning> SeancesPlanning { get; set; } = new List<SeancePlanning>();
    }
}