using System.Collections.Generic;

namespace centre_soutien.Models
{
    public class Paiement
    {
        public int IDPaiement { get; set; }
        public int IDEtudiant { get; set; }
        public int IDUtilisateurEnregistrement { get; set; }
        public string DatePaiement { get; set; }= string.Empty;
        public double MontantTotalRecuTransaction { get; set; }
        public string Notes { get; set; }= string.Empty;

        // Navigation vers les entités "parentes" et "enfants"
        public virtual Etudiant Etudiant { get; set; }
        public virtual Utilisateur UtilisateurEnregistrement { get; set; }
        public virtual ICollection<DetailPaiement> DetailsPaiements { get; set; } = new List<DetailPaiement>();
    }
}