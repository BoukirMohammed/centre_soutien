using System;
using System.Globalization;

namespace centre_soutien.Models
{
    /// <summary>
    /// Classe pour représenter une échéance avec ses informations de paiement
    /// </summary>
    public class EcheanceInfo
    {
        public Inscription Inscription { get; set; } = new Inscription();
        public string MoisConcerne { get; set; } = string.Empty; // Format "yyyy-MM"
        public DateTime DateEcheance { get; set; }
        public double MontantDu { get; set; }
        public double MontantDejaPaye { get; set; }
        public double MontantRestant { get; set; }
        public bool EstEnRetard { get; set; }
        public bool EstPayeCompletement { get; set; }
        
        // Propriétés calculées pour l'affichage
        public string StatutTexte => EstPayeCompletement ? "Payé" : EstEnRetard ? "En retard" : "À payer";
        public string MoisFormate => DateTime.ParseExact(MoisConcerne + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("MMMM yyyy", new CultureInfo("fr-FR"));
    }
}