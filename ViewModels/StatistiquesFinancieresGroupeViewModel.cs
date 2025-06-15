using centre_soutien.Models;
using centre_soutien.DataAccess;
using centre_soutien.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;
using System.Globalization;

namespace centre_soutien.ViewModels
{
    public class StatistiquesFinancieresGroupeViewModel : INotifyPropertyChanged
    {
        private readonly PaiementRepository _paiementRepository;
        private readonly GroupeRepository _groupeRepository;
        private readonly InscriptionRepository _inscriptionRepository;
        private readonly ProfesseurMatiereRepository _professeurMatiereRepository;
        private readonly int _groupeId;

        // Groupe concerné
        private Groupe? _groupeConcerne;
        public Groupe? GroupeConcerne
        {
            get => _groupeConcerne;
            set { _groupeConcerne = value; OnPropertyChanged(); }
        }

        // Période d'analyse
        private DateTime _dateDebut;
        public DateTime DateDebut
        {
            get => _dateDebut;
            set 
            { 
                _dateDebut = value; 
                OnPropertyChanged(); 
                _ = CalculerStatistiquesAsync();
            }
        }

        private DateTime _dateFin;
        public DateTime DateFin
        {
            get => _dateFin;
            set 
            { 
                _dateFin = value; 
                OnPropertyChanged(); 
                _ = CalculerStatistiquesAsync();
            }
        }

        // Statistiques calculées
        private double _montantTotalCollecte;
        public double MontantTotalCollecte
        {
            get => _montantTotalCollecte;
            set 
            { 
                _montantTotalCollecte = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(MontantTotalCollecteFormate)); 
            }
        }

        private double _montantProfesseur;
        public double MontantProfesseur
        {
            get => _montantProfesseur;
            set 
            { 
                _montantProfesseur = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(MontantProfesseurFormate)); 
            }
        }

        private double _montantAttendu;
        public double MontantAttendu
        {
            get => _montantAttendu;
            set 
            { 
                _montantAttendu = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(MontantAttenduFormate)); 
            }
        }

        private double _montantEnRetard;
        public double MontantEnRetard
        {
            get => _montantEnRetard;
            set 
            { 
                _montantEnRetard = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(MontantEnRetardFormate));
                OnPropertyChanged(nameof(CouleurMontantEnRetard));
            }
        }

        private double _profitCentre;
        public double ProfitCentre
        {
            get => _profitCentre;
            set 
            { 
                _profitCentre = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(ProfitCentreFormate));
                OnPropertyChanged(nameof(CouleurProfitCentre));
            }
        }

        // Informations complémentaires
        private int _nombreEtudiantsActifs;
        public int NombreEtudiantsActifs
        {
            get => _nombreEtudiantsActifs;
            set { _nombreEtudiantsActifs = value; OnPropertyChanged(); }
        }

        private double _pourcentageRemunerationProfesseur;
        public double PourcentageRemunerationProfesseur
        {
            get => _pourcentageRemunerationProfesseur;
            set 
            { 
                _pourcentageRemunerationProfesseur = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(PourcentageRemunerationFormate)); 
            }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        // Propriétés formatées pour l'affichage (LECTURE SEULE)
        public string MontantTotalCollecteFormate => MontantTotalCollecte.ToString("C", new CultureInfo("fr-MA"));
        public string MontantProfesseurFormate => MontantProfesseur.ToString("C", new CultureInfo("fr-MA"));
        public string MontantAttenduFormate => MontantAttendu.ToString("C", new CultureInfo("fr-MA"));
        public string MontantEnRetardFormate => MontantEnRetard.ToString("C", new CultureInfo("fr-MA"));
        public string ProfitCentreFormate => ProfitCentre.ToString("C", new CultureInfo("fr-MA"));
        public string PourcentageRemunerationFormate => $"{PourcentageRemunerationProfesseur:F1}%";

        // Propriétés pour les couleurs d'affichage (LECTURE SEULE)
        public string CouleurMontantEnRetard => MontantEnRetard > 0 ? "#e53e3e" : "#38a169";
        public string CouleurProfitCentre => ProfitCentre >= 0 ? "#38a169" : "#e53e3e";

        // Détail des paiements
        private ObservableCollection<Paiement> _paiementsPeriode;
        public ObservableCollection<Paiement> PaiementsPeriode
        {
            get => _paiementsPeriode;
            set { _paiementsPeriode = value; OnPropertyChanged(); }
        }

        // Commandes
        public ICommand ActualiserCommand { get; }
        public ICommand ExporterCommand { get; }

        public StatistiquesFinancieresGroupeViewModel(int groupeId)
        {
            _groupeId = groupeId;
            _paiementRepository = new PaiementRepository();
            _groupeRepository = new GroupeRepository();
            _inscriptionRepository = new InscriptionRepository();
            _professeurMatiereRepository = new ProfesseurMatiereRepository();

            PaiementsPeriode = new ObservableCollection<Paiement>();

            // Initialiser la période par défaut (mois courant)
            var maintenant = DateTime.Now;
            DateDebut = new DateTime(maintenant.Year, maintenant.Month, 1);
            DateFin = new DateTime(maintenant.Year, maintenant.Month, DateTime.DaysInMonth(maintenant.Year, maintenant.Month));

            ActualiserCommand = new RelayCommand(async param => await CalculerStatistiquesAsync());
            ExporterCommand = new RelayCommand(param => ExporterStatistiques(), CanExporter);

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            StatusMessage = "Chargement des données du groupe...";
            try
            {
                // Charger les informations du groupe
                GroupeConcerne = await _groupeRepository.GetGroupeByIdAsync(_groupeId);
                
                if (GroupeConcerne == null)
                {
                    StatusMessage = "Groupe non trouvé.";
                    return;
                }

                // Charger le pourcentage de rémunération du professeur
                var associationProfMatiere = await _professeurMatiereRepository.GetAssociationAsync(
                    GroupeConcerne.IDProfesseur, 
                    GroupeConcerne.IDMatiere);
                
                PourcentageRemunerationProfesseur = associationProfMatiere?.PourcentageRemuneration ?? 0;

                // Calculer les statistiques
                await CalculerStatistiquesAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement : {ex.Message}";
            }
        }

        private async Task CalculerStatistiquesAsync()
        {
            if (GroupeConcerne == null) return;

            StatusMessage = "Calcul des statistiques en cours...";

            try
            {
                // 1. Obtenir les inscriptions actives du groupe
                var inscriptionsActives = await _inscriptionRepository.GetActiveInscriptionsForGroupeAsync(_groupeId);
                NombreEtudiantsActifs = inscriptionsActives.Count;

                // 2. Calculer le montant total collecté
                MontantTotalCollecte = await CalculerMontantTotalCollecteAsync(inscriptionsActives);

                // 3. Calculer le montant professeur
                MontantProfesseur = MontantTotalCollecte * (PourcentageRemunerationProfesseur / 100.0);

                // 4. Calculer le montant attendu
                MontantAttendu = await CalculerMontantAttenduAsync(inscriptionsActives);

                // 5. Calculer le montant en retard
                MontantEnRetard = Math.Max(0, MontantAttendu - MontantTotalCollecte);

                // 6. Calculer le profit du centre
                ProfitCentre = MontantTotalCollecte - MontantProfesseur;

                // 7. Charger le détail des paiements
                await ChargerDetailPaiementsAsync(inscriptionsActives);

                StatusMessage = $"Statistiques calculées pour la période du {DateDebut:dd/MM/yyyy} au {DateFin:dd/MM/yyyy}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du calcul : {ex.Message}";
            }
        }

        private async Task<double> CalculerMontantTotalCollecteAsync(List<Inscription> inscriptions)
        {
            double totalCollecte = 0;

            foreach (var inscription in inscriptions)
            {
                // Obtenir tous les paiements pour cette inscription dans la période
                var paiements = await _paiementRepository.GetPaiementsForEtudiantAsync(inscription.IDEtudiant);
                
                foreach (var paiement in paiements)
                {
                    var datePaiement = DateTime.ParseExact(paiement.DatePaiement, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    
                    if (datePaiement >= DateDebut && datePaiement <= DateFin)
                    {
                        // Sommer seulement les détails qui concernent cette inscription
                        var detailsPourInscription = paiement.DetailsPaiements?
                            .Where(dp => dp.IDInscription == inscription.IDInscription)
                            .Sum(dp => dp.MontantPayePourEcheance) ?? 0;
                        
                        totalCollecte += detailsPourInscription;
                    }
                }
            }

            return totalCollecte;
        }

        private async Task<double> CalculerMontantAttenduAsync(List<Inscription> inscriptions)
        {
            double totalAttendu = 0;

            foreach (var inscription in inscriptions)
            {
                var dateInscription = DateTime.ParseExact(inscription.DateInscription, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                
                // Calculer le nombre de mois dans la période pour cette inscription
                var debutPeriodePourInscription = dateInscription > DateDebut ? dateInscription : DateDebut;
                var finPeriodePourInscription = DateFin;

                if (debutPeriodePourInscription <= finPeriodePourInscription)
                {
                    // Calculer le nombre de mois (approximatif)
                    var nombreMois = Math.Max(1, 
                        ((finPeriodePourInscription.Year - debutPeriodePourInscription.Year) * 12) + 
                        finPeriodePourInscription.Month - debutPeriodePourInscription.Month + 1);

                    totalAttendu += inscription.PrixConvenuMensuel * nombreMois;
                }
            }

            return totalAttendu;
        }

        private async Task ChargerDetailPaiementsAsync(List<Inscription> inscriptions)
        {
            var paiements = new List<Paiement>();

            foreach (var inscription in inscriptions)
            {
                var paiementsEtudiant = await _paiementRepository.GetPaiementsForEtudiantAsync(inscription.IDEtudiant);
                
                foreach (var paiement in paiementsEtudiant)
                {
                    var datePaiement = DateTime.ParseExact(paiement.DatePaiement, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    
                    if (datePaiement >= DateDebut && datePaiement <= DateFin)
                    {
                        // Vérifier si ce paiement concerne notre groupe
                        var aConcerneGroupe = paiement.DetailsPaiements?.Any(dp => 
                            inscriptions.Any(i => i.IDInscription == dp.IDInscription)) ?? false;
                        
                        if (aConcerneGroupe && !paiements.Any(p => p.IDPaiement == paiement.IDPaiement))
                        {
                            paiements.Add(paiement);
                        }
                    }
                }
            }

            PaiementsPeriode = new ObservableCollection<Paiement>(paiements.OrderByDescending(p => p.DatePaiement));
        }

        private bool CanExporter(object? parameter)
        {
            return GroupeConcerne != null && PaiementsPeriode.Any();
        }

        private void ExporterStatistiques()
        {
            // TODO: Implémenter l'export (CSV, PDF, etc.)
            StatusMessage = "Fonctionnalité d'export à implémenter";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}