using centre_soutien.Models;
using centre_soutien.DataAccess;
using centre_soutien.Commands;
using centre_soutien.Services;
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
    public class PaiementViewModel : INotifyPropertyChanged
    {
        private readonly PaiementRepository _paiementRepository;
        private readonly EtudiantRepository _etudiantRepository;

        // Collections pour les ComboBox
        private ObservableCollection<Etudiant> _allEtudiants;
        public ObservableCollection<Etudiant> AllEtudiants
        {
            get => _allEtudiants;
            set { _allEtudiants = value; OnPropertyChanged(); }
        }

        // Étudiant sélectionné
        private Etudiant? _selectedEtudiant;
        public Etudiant? SelectedEtudiant
        {
            get => _selectedEtudiant;
            set 
            { 
                _selectedEtudiant = value; 
                OnPropertyChanged(); 
                if (_selectedEtudiant != null)
                {
                    _ = LoadEcheancesForSelectedEtudiantAsync();
                    _ = LoadStatistiquesEtudiantAsync();
                }
                else
                {
                    EcheancesEtudiant.Clear();
                    StatistiquesEtudiant = null;
                }
                (EnregistrerPaiementCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Échéances de l'étudiant sélectionné
        private ObservableCollection<EcheanceDisplayItem> _echeancesEtudiant;
        public ObservableCollection<EcheanceDisplayItem> EcheancesEtudiant
        {
            get => _echeancesEtudiant;
            set { _echeancesEtudiant = value; OnPropertyChanged(); }
        }

        // Statistiques de l'étudiant
        private StatistiquesPaiementEtudiant? _statistiquesEtudiant;
        public StatistiquesPaiementEtudiant? StatistiquesEtudiant
        {
            get => _statistiquesEtudiant;
            set { _statistiquesEtudiant = value; OnPropertyChanged(); }
        }

        // Propriétés pour le nouveau paiement
        private DateTime _datePaiement = DateTime.Today;
        public DateTime DatePaiement
        {
            get => _datePaiement;
            set { _datePaiement = value; OnPropertyChanged(); }
        }

        private double _montantTotal;
        public double MontantTotal
        {
            get => _montantTotal;
            set { _montantTotal = value; OnPropertyChanged(); OnPropertyChanged(nameof(MontantTotalFormate)); }
        }

        public string MontantTotalFormate => MontantTotal.ToString("C", new CultureInfo("fr-MA"));

        private string _notesPaiement = string.Empty;
        public string NotesPaiement
        {
            get => _notesPaiement;
            set { _notesPaiement = value; OnPropertyChanged(); }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        // Historique des paiements
        private ObservableCollection<Paiement> _historiquePaiements;
        public ObservableCollection<Paiement> HistoriquePaiements
        {
            get => _historiquePaiements;
            set { _historiquePaiements = value; OnPropertyChanged(); }
        }

        // Commandes
        public ICommand LoadDataCommand { get; }
        public ICommand EnregistrerPaiementCommand { get; }
        public ICommand CalculerMontantAutoCommand { get; }
        public ICommand LoadHistoriqueCommand { get; }
        public ICommand SupprimerPaiementCommand { get; }

        public PaiementViewModel()
        {
            _paiementRepository = new PaiementRepository();
            _etudiantRepository = new EtudiantRepository();

            AllEtudiants = new ObservableCollection<Etudiant>();
            EcheancesEtudiant = new ObservableCollection<EcheanceDisplayItem>();
            HistoriquePaiements = new ObservableCollection<Paiement>();

            LoadDataCommand = new RelayCommand(async param => await LoadAllDataAsync());
            EnregistrerPaiementCommand = new RelayCommand(async param => await EnregistrerPaiementAsync(), CanEnregistrerPaiement);
            CalculerMontantAutoCommand = new RelayCommand(param => CalculerMontantAutomatique());
            LoadHistoriqueCommand = new RelayCommand(async param => await LoadHistoriquePaiementsAsync());
            SupprimerPaiementCommand = new RelayCommand(async param => await SupprimerPaiementAsync(param), CanSupprimerPaiement);

            _ = LoadAllDataAsync();
        }

        private async Task LoadAllDataAsync()
        {
            try
            {
                var etudiants = await _etudiantRepository.GetAllEtudiantsAsync();
                AllEtudiants = new ObservableCollection<Etudiant>(etudiants.OrderBy(e => e.Nom).ThenBy(e => e.Prenom));
                StatusMessage = $"Chargement de {AllEtudiants.Count} étudiants réussi.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement des données : {ex.Message}";
            }
        }

        private async Task LoadEcheancesForSelectedEtudiantAsync()
        {
            if (SelectedEtudiant == null) return;

            try
            {
                var echeances = await _paiementRepository.GetEcheancesAPayer(SelectedEtudiant.IDEtudiant, DateTime.Now);
                
                var echeancesDisplay = echeances.Select(e => new EcheanceDisplayItem
                {
                    EcheanceInfo = e,
                    MontantAPayer = e.MontantRestant,
                    EstSelectionne = e.EstEnRetard || (e.DateEcheance <= DateTime.Now.AddDays(7) && e.MontantRestant > 0) // Auto-sélectionner les retards et échéances proches
                }).ToList();

                EcheancesEtudiant = new ObservableCollection<EcheanceDisplayItem>(echeancesDisplay);
                
                // Calculer automatiquement le montant total
                CalculerMontantAutomatique();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement des échéances : {ex.Message}";
            }
        }

        private async Task LoadStatistiquesEtudiantAsync()
        {
            if (SelectedEtudiant == null) return;

            try
            {
                StatistiquesEtudiant = await _paiementRepository.GetStatistiquesPaiementEtudiantAsync(SelectedEtudiant.IDEtudiant);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement des statistiques : {ex.Message}";
            }
        }

        private void CalculerMontantAutomatique()
        {
            if (EcheancesEtudiant?.Any() != true) return;

            var montantCalcule = EcheancesEtudiant
                .Where(e => e.EstSelectionne)
                .Sum(e => e.MontantAPayer);

            MontantTotal = montantCalcule;
        }

        private bool CanEnregistrerPaiement(object? parameter)
        {
            return SelectedEtudiant != null && 
                   MontantTotal > 0 && 
                   EcheancesEtudiant?.Any(e => e.EstSelectionne && e.MontantAPayer > 0) == true;
        }

        private async Task EnregistrerPaiementAsync()
        {
            if (!CanEnregistrerPaiement(null) || SelectedEtudiant == null) return;

            try
            {
                // Validation : vérifier que le montant total correspond aux échéances sélectionnées
                var montantEcheances = EcheancesEtudiant
                    .Where(e => e.EstSelectionne)
                    .Sum(e => e.MontantAPayer);

                if (Math.Abs(MontantTotal - montantEcheances) > 0.01)
                {
                    StatusMessage = "Le montant total ne correspond pas à la somme des échéances sélectionnées.";
                    return;
                }

                // Créer le paiement principal
                var paiement = new Paiement
                {
                    IDEtudiant = SelectedEtudiant.IDEtudiant,
                    IDUtilisateurEnregistrement = CurrentUserSession.CurrentUser?.IDUtilisateur ?? 1,
                    DatePaiement = DatePaiement.ToString("yyyy-MM-dd"),
                    MontantTotalRecuTransaction = MontantTotal,
                    Notes = NotesPaiement
                };

                // Créer les détails du paiement
                var details = EcheancesEtudiant
                    .Where(e => e.EstSelectionne && e.MontantAPayer > 0)
                    .Select(e => new DetailPaiement
                    {
                        IDInscription = e.EcheanceInfo.Inscription.IDInscription,
                        AnneeMoisConcerne = e.EcheanceInfo.MoisConcerne,
                        MontantPayePourEcheance = e.MontantAPayer
                    }).ToList();

                // Enregistrer en base de données
                await _paiementRepository.AddPaiementAsync(paiement, details);

                StatusMessage = $"Paiement de {MontantTotal:C} enregistré avec succès pour {SelectedEtudiant.NomComplet}.";

                // Rafraîchir les données
                await LoadEcheancesForSelectedEtudiantAsync();
                await LoadStatistiquesEtudiantAsync();
                await LoadHistoriquePaiementsAsync();

                // Réinitialiser le formulaire
                ResetFormulairePaiement();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors de l'enregistrement du paiement : {ex.Message}";
            }
        }

        private async Task LoadHistoriquePaiementsAsync()
        {
            if (SelectedEtudiant == null) return;

            try
            {
                var historique = await _paiementRepository.GetPaiementsForEtudiantAsync(SelectedEtudiant.IDEtudiant);
                HistoriquePaiements = new ObservableCollection<Paiement>(historique);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement de l'historique : {ex.Message}";
            }
        }

        private bool CanSupprimerPaiement(object? parameter)
        {
            return parameter is Paiement && CurrentUserSession.IsAdmin;
        }

        private async Task SupprimerPaiementAsync(object? parameter)
        {
            if (parameter is not Paiement paiement) return;

            try
            {
                await _paiementRepository.SupprimerPaiementAsync(paiement.IDPaiement);
                StatusMessage = $"Paiement du {paiement.DatePaiement} supprimé avec succès.";

                // Rafraîchir les données
                await LoadEcheancesForSelectedEtudiantAsync();
                await LoadStatistiquesEtudiantAsync();
                await LoadHistoriquePaiementsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors de la suppression : {ex.Message}";
            }
        }

        private void ResetFormulairePaiement()
        {
            MontantTotal = 0;
            NotesPaiement = string.Empty;
            DatePaiement = DateTime.Today;
            
            // Décocher toutes les échéances
            foreach (var echeance in EcheancesEtudiant)
            {
                echeance.EstSelectionne = false;
                echeance.MontantAPayer = echeance.EcheanceInfo.MontantRestant;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Classe pour afficher les échéances dans l'interface avec possibilité de sélection et modification du montant
    /// </summary>
    public class EcheanceDisplayItem : INotifyPropertyChanged
    {
        public EcheanceInfo EcheanceInfo { get; set; } = new EcheanceInfo();

        private bool _estSelectionne;
        public bool EstSelectionne
        {
            get => _estSelectionne;
            set { _estSelectionne = value; OnPropertyChanged(); }
        }

        private double _montantAPayer;
        public double MontantAPayer
        {
            get => _montantAPayer;
            set { _montantAPayer = value; OnPropertyChanged(); OnPropertyChanged(nameof(MontantAPayerFormate)); }
        }

        public string MontantAPayerFormate => MontantAPayer.ToString("C", new CultureInfo("fr-MA"));
        
        // Propriétés pour l'affichage
        public string InscriptionDisplay => $"{EcheanceInfo.Inscription?.Groupe?.NomDescriptifGroupe} - {EcheanceInfo.Inscription?.Groupe?.Matiere?.NomMatiere}";
        public string StatutDisplay => EcheanceInfo.StatutTexte;
        public string MontantDuFormate => EcheanceInfo.MontantDu.ToString("C", new CultureInfo("fr-MA"));
        public string MontantPayeFormate => EcheanceInfo.MontantDejaPaye.ToString("C", new CultureInfo("fr-MA"));
        public string MontantRestantFormate => EcheanceInfo.MontantRestant.ToString("C", new CultureInfo("fr-MA"));
        
        // Couleurs pour l'affichage selon le statut
        public string CouleurStatut => EcheanceInfo.EstPayeCompletement ? "#38a169" : 
                                      EcheanceInfo.EstEnRetard ? "#e53e3e" : "#4299e1";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}