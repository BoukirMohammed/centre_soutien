using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace centre_soutien.Models
{
    /// <summary>
    /// Représente l'état des paiements d'un étudiant pour un mois donné
    /// </summary>
    public class StatutPaiementMensuel : INotifyPropertyChanged
    {
        public int EtudiantId { get; set; }
        public int Annee { get; set; }
        public int Mois { get; set; }
        public string NomMois { get; set; } = string.Empty;
        
        private double _montantDu;
        public double MontantDu 
        { 
            get => _montantDu; 
            set 
            { 
                _montantDu = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(MontantRestant));
                OnPropertyChanged(nameof(EstCompletementPaye));
                OnPropertyChanged(nameof(EstPartiellementPaye));
                OnPropertyChanged(nameof(PourcentagePaye));
                OnPropertyChanged(nameof(StatutTexte));
                OnPropertyChanged(nameof(StatutCouleur));
                OnPropertyChanged(nameof(StatutIcon));
            } 
        }
        
        private double _montantPaye;
        public double MontantPaye 
        { 
            get => _montantPaye; 
            set 
            { 
                _montantPaye = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(MontantRestant));
                OnPropertyChanged(nameof(EstCompletementPaye));
                OnPropertyChanged(nameof(EstPartiellementPaye));
                OnPropertyChanged(nameof(PourcentagePaye));
                OnPropertyChanged(nameof(StatutTexte));
                OnPropertyChanged(nameof(StatutCouleur));
                OnPropertyChanged(nameof(StatutIcon));
            } 
        }

        // Propriétés calculées
        public double MontantRestant => Math.Max(0, MontantDu - MontantPaye);

        
        public bool EstCompletementPaye => MontantDu > 0 && MontantPaye >= MontantDu;
        
        public bool EstPartiellementPaye => MontantDu > 0 && MontantPaye > 0 && MontantPaye < MontantDu;
        
        public bool EstImpaye => MontantDu > 0 && MontantPaye == 0;
        
        public double PourcentagePaye => MontantDu > 0 ? (MontantPaye / MontantDu) * 100 : 0;

        // Propriétés pour l'affichage dans l'interface utilisateur
        public string StatutTexte
        {
            get
            {
                if (MontantDu == 0) return "Aucun paiement dû";
                if (EstCompletementPaye) return "Payé";
                if (EstPartiellementPaye) return "Partiel";
                return "Impayé";
            }
        }

        public string StatutCouleur
        {
            get
            {
                if (MontantDu == 0) return "#718096"; // Gris
                if (EstCompletementPaye) return "#48bb78"; // Vert
                if (EstPartiellementPaye) return "#ed8936"; // Orange
                return "#f56565"; // Rouge
            }
        }

        public string StatutIcon
        {
            get
            {
                if (MontantDu == 0) return "➖";
                if (EstCompletementPaye) return "✅";
                if (EstPartiellementPaye) return "⚠️";
                return "❌";
            }
        }

        public string MontantDuFormate => $"{MontantDu:C} DH";
        public string MontantPayeFormate => $"{MontantPaye:C} DH";
        public string MontantRestantFormate => $"{MontantRestant:C} DH";

        /// <summary>
        /// Retourne une description détaillée du statut
        /// </summary>
        public string DescriptionComplete
        {
            get
            {
                if (MontantDu == 0)
                    return $"{NomMois} {Annee} : Aucun montant dû";
                
                if (EstCompletementPaye)
                    return $"{NomMois} {Annee} : Entièrement payé ({MontantPayeFormate})";
                
                if (EstPartiellementPaye)
                    return $"{NomMois} {Annee} : Partiellement payé ({MontantPayeFormate} sur {MontantDuFormate})";
                
                return $"{NomMois} {Annee} : Impayé ({MontantDuFormate})";
            }
        }

        /// <summary>
        /// Date du mois concerné (premier jour du mois)
        /// </summary>
        public DateTime DateMois => new DateTime(Annee, Mois, 1);

        /// <summary>
        /// Indique si ce mois est dans le futur
        /// </summary>
        public bool EstDansFutur => DateMois > DateTime.Now.Date;

        /// <summary>
        /// Indique si ce mois est le mois actuel
        /// </summary>
        public bool EstMoisActuel => DateMois.Year == DateTime.Now.Year && DateMois.Month == DateTime.Now.Month;

        /// <summary>
        /// Indique si ce mois est en retard (passé et non payé)
        /// </summary>
        public bool EstEnRetard => DateMois < DateTime.Now.Date && MontantRestant > 0;

        /// <summary>
        /// Code couleur pour l'urgence (utile pour le tri)
        /// </summary>
        public int CodeUrgence
        {
            get
            {
                if (EstCompletementPaye) return 0; // Pas urgent
                if (EstDansFutur) return 1; // Peu urgent
                if (EstMoisActuel) return 2; // Moyennement urgent
                if (EstEnRetard) return 3; // Très urgent
                return 0;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public int? InscriptionId { get; set; } // Nullable pour compatibilité avec les stats globales
    
        // Propriétés pour l'affichage dans le calendrier
        public string Icone => StatutIcon; // Réutilise ton StatutIcon existant
        public string CouleurFond => StatutCouleur; // Réutilise ton StatutCouleur existant
    
        // Tooltip informatif pour le calendrier
        public string Tooltip 
        { 
            get
            {
                var tooltip = $"{NomMois} {Annee}\n";
                tooltip += $"Dû: {MontantDu:F2} DH\n";
                tooltip += $"Payé: {MontantPaye:F2} DH";
            
                if (MontantRestant > 0)
                    tooltip += $"\nRestant: {MontantRestant:F2} DH";
                
                if (EstEnRetard)
                    tooltip += "\n⚠️ EN RETARD";
                else if (EstCompletementPaye)
                    tooltip += "\n✅ PAYÉ";
                else if (EstPartiellementPaye)
                    tooltip += "\n⚠️ PARTIEL";
                
                return tooltip;
            }
        }
    
        // Format d'affichage pour les détails
        public string MontantFormate => $"{MontantPaye:F2} DH / {MontantDu:F2} DH";
    }
}