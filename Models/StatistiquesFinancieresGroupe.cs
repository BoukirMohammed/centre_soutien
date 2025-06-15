using System;
using System.Collections.Generic;
using System.Globalization;

namespace centre_soutien.Models
{
    /// <summary>
    /// Classe pour encapsuler toutes les statistiques financières d'un groupe
    /// </summary>
    public class StatistiquesFinancieresGroupe
    {
        public Groupe Groupe { get; set; } = new Groupe();
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public int NombreEtudiantsActifs { get; set; }
        public double MontantTotalCollecte { get; set; }
        public double MontantAttendu { get; set; }
        public double MontantProfesseur { get; set; }
        public double MontantEnRetard { get; set; }
        public double ProfitCentre { get; set; }
        public double PourcentageRemunerationProfesseur { get; set; }
        public double TauxCollecte { get; set; }
        public List<Paiement> PaiementsPeriode { get; set; } = new List<Paiement>();

        // Propriétés calculées pour l'affichage
        public string PeriodeAffichage => $"{DateDebut:dd/MM/yyyy} - {DateFin:dd/MM/yyyy}";
        public string MontantTotalCollecteFormate => MontantTotalCollecte.ToString("C", new CultureInfo("fr-MA"));
        public string MontantAttenduFormate => MontantAttendu.ToString("C", new CultureInfo("fr-MA"));
        public string MontantProfesseurFormate => MontantProfesseur.ToString("C", new CultureInfo("fr-MA"));
        public string MontantEnRetardFormate => MontantEnRetard.ToString("C", new CultureInfo("fr-MA"));
        public string ProfitCentreFormate => ProfitCentre.ToString("C", new CultureInfo("fr-MA"));
        public string TauxCollecteFormate => $"{TauxCollecte:F1}%";
        public string PourcentageRemunerationFormate => $"{PourcentageRemunerationProfesseur:F1}%";

        // Propriétés pour les indicateurs visuels
        public string CouleurMontantEnRetard => MontantEnRetard > 0 ? "#e53e3e" : "#38a169";
        public string CouleurProfitCentre => ProfitCentre >= 0 ? "#38a169" : "#e53e3e";
        public string CouleurTauxCollecte => TauxCollecte >= 90 ? "#38a169" : TauxCollecte >= 70 ? "#dd6b20" : "#e53e3e";

        // Indicateurs de performance
        public bool EstRentable => ProfitCentre > 0;
        public bool ADesRetards => MontantEnRetard > 0;
        public bool TauxCollecteAcceptable => TauxCollecte >= 70;

        // Résumé textuel
        public string ResumePerfomance
        {
            get
            {
                if (TauxCollecte >= 90 && EstRentable)
                    return "✅ Excellent - Groupe très performant";
                else if (TauxCollecte >= 70 && EstRentable)
                    return "✅ Bien - Performance satisfaisante";
                else if (TauxCollecte >= 50)
                    return "⚠️ Moyen - Nécessite une attention";
                else
                    return "❌ Faible - Problèmes de paiement importants";
            }
        }
    }

    /// <summary>
    /// Classe pour les détails d'un paiement dans le contexte des statistiques
    /// </summary>
    public class DetailPaiementStatistique
    {
        public int IDPaiement { get; set; }
        public DateTime DatePaiement { get; set; }
        public string NomEtudiant { get; set; } = string.Empty;
        public double MontantTotal { get; set; }
        public double MontantPourGroupe { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string UtilisateurEnregistrement { get; set; } = string.Empty;

        public string DatePaiementFormatee => DatePaiement.ToString("dd/MM/yyyy");
        public string MontantTotalFormate => MontantTotal.ToString("C", new CultureInfo("fr-MA"));
        public string MontantPourGroupeFormate => MontantPourGroupe.ToString("C", new CultureInfo("fr-MA"));
    }

    /// <summary>
    /// Classe pour l'analyse comparative entre groupes (extension future)
    /// </summary>
    public class ComparaisonGroupes
    {
        public StatistiquesFinancieresGroupe GroupePrincipal { get; set; } = new StatistiquesFinancieresGroupe();
        public List<StatistiquesFinancieresGroupe> AutresGroupes { get; set; } = new List<StatistiquesFinancieresGroupe>();
        public double MoyenneTauxCollecte { get; set; }
        public double MoyenneProfitCentre { get; set; }
        public int PositionClassement { get; set; }
    }
}