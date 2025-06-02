using System.Collections.Generic; // Pour ICollection

namespace centre_soutien.Models
{
    public class Utilisateur
    {
        public int IDUtilisateur { get; set; }
        public string Login { get; set; }= string.Empty;
        public string MotDePasseHashe { get; set; }= string.Empty;
        public string Role { get; set; }= string.Empty;
        public string NomComplet { get; set; }= string.Empty;
        public bool EstActif { get; set; }

        // Navigation: Un utilisateur (secrétaire/admin) peut avoir enregistré plusieurs paiements
        public virtual ICollection<Paiement> PaiementsEnregistres { get; set; } = new List<Paiement>();
    }
}