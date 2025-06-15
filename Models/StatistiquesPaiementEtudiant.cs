using System;

namespace centre_soutien.Models
{
    /// <summary>
    /// Statistiques de paiement pour un étudiant
    /// </summary>
    public class StatistiquesPaiementEtudiant
    {
        public double TotalPaye { get; set; }
        public int NombrePaiements { get; set; }
        public DateTime? DernierPaiement { get; set; }
        public double MontantEnRetard { get; set; }
        public int NombreMoisEnRetard { get; set; }
        public bool EstAJour => MontantEnRetard <= 0;
    }
}