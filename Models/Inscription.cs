using System.Collections.Generic;

namespace centre_soutien.Models
{
    public class Inscription
    {
        public int IDInscription { get; set; }
        public int IDEtudiant { get; set; }
        public int IDGroupe { get; set; }
        public string DateInscription { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");   
        public double PrixConvenuMensuel { get; set; }
        public int JourEcheanceMensuelle { get; set; }
        
        public bool EstActif { get; set; } = true; // Par défaut, une nouvelle inscription est active
        public string? DateDesinscription { get; set; } // Nullable, format "YYYY-MM-DD"

        // Navigation vers les entités "parentes" et "enfants"
        public virtual Etudiant? Etudiant { get; set; }
        public virtual Groupe? Groupe { get; set; }
        public virtual ICollection<DetailPaiement> DetailsPaiements { get; set; } = new List<DetailPaiement>();

        public Inscription()
        {
            // DateInscription est initialisée ci-dessus.
            // EstActif est initialisé à true.
        }
    }
}