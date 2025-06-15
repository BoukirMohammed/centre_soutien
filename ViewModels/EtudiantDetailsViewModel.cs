using centre_soutien.Models;
using centre_soutien.DataAccess;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Input;

namespace centre_soutien.ViewModels
{
    public class EtudiantDetailsViewModel : INotifyPropertyChanged
    {
        private readonly InscriptionRepository _inscriptionRepository;
        private readonly PaiementRepository _paiementRepository;

        // Étudiant principal
        private Etudiant _etudiantActuel;
        public Etudiant EtudiantActuel
        {
            get => _etudiantActuel;
            set
            {
                _etudiantActuel = value;
                OnPropertyChanged();
            }
        }
        

        // Inscriptions actives (cours suivis)
        private ObservableCollection<Inscription> _inscriptionsActives;
        public ObservableCollection<Inscription> InscriptionsActives
        {
            get => _inscriptionsActives;
            set
            {
                _inscriptionsActives = value;
                OnPropertyChanged();
            }
        }

        // État des paiements par mois
        private ObservableCollection<StatutPaiementMensuel> _etatPaiements;
        public ObservableCollection<StatutPaiementMensuel> EtatPaiements
        {
            get => _etatPaiements;
            set
            {
                _etatPaiements = value;
                OnPropertyChanged();
            }
        }

        // Nouvelle propriété pour la matière sélectionnée
        private Inscription _selectedMatiere;
        public Inscription SelectedMatiere
        {
            get => _selectedMatiere;
            set
            {
                // Désélectionner l'ancienne matière
                if (_selectedMatiere != null)
                    _selectedMatiere.IsSelected = false;

                _selectedMatiere = value;

                // Sélectionner la nouvelle matière
                if (_selectedMatiere != null)
                    _selectedMatiere.IsSelected = true;

                OnPropertyChanged();
                OnPropertyChanged(nameof(EtatPaiementsMatiere));
                OnPropertyChanged(nameof(MoisPayesMatiere));
                OnPropertyChanged(nameof(MoisPartielsMatiere));
                OnPropertyChanged(nameof(MoisImpayesMatiere));

                // Charger les paiements de cette matière
                _ = LoadEtatPaiementsMatiereAsync();
            }
        }

        // Collection des paiements pour la matière sélectionnée
        private ObservableCollection<StatutPaiementMensuel> _etatPaiementsMatiere;
        public ObservableCollection<StatutPaiementMensuel> EtatPaiementsMatiere
        {
            get => _etatPaiementsMatiere ??= new ObservableCollection<StatutPaiementMensuel>();
            set
            {
                _etatPaiementsMatiere = value;
                OnPropertyChanged();
            }
        }

        // Année en cours pour les paiements
        private int _anneeSelectionnee;
        public int AnneeSelectionnee
        {
            get => _anneeSelectionnee;
            set
            {
                _anneeSelectionnee = value;
                OnPropertyChanged();
                _ = LoadEtatPaiementsAsync(); // Recharger les paiements pour la nouvelle année
                _ = LoadEtatPaiementsMatiereAsync(); // Recharger aussi pour la matière sélectionnée
            }
        }

        // Message de statut
        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        // Propriétés calculées pour l'affichage
        public string NomCompletEtudiant => $"{EtudiantActuel?.Prenom} {EtudiantActuel?.Nom}";

        public string AgeEtudiant
        {
            get
            {
                if (EtudiantActuel?.DateNaissance == null ||
                    !DateTime.TryParse(EtudiantActuel.DateNaissance, out DateTime dateNaissance))
                    return "Non renseigné";

                var age = DateTime.Now.Year - dateNaissance.Year;
                if (DateTime.Now.DayOfYear < dateNaissance.DayOfYear) age--;
                return $"{age} ans";
            }
        }

        public string DateInscriptionFormatee
        {
            get
            {
                if (EtudiantActuel?.DateInscriptionSysteme == null ||
                    !DateTime.TryParse(EtudiantActuel.DateInscriptionSysteme, out DateTime dateInscription))
                    return "Non renseignée";

                return dateInscription.ToString("dd/MM/yyyy");
            }
        }

        public int NombreCoursSuivis => InscriptionsActives?.Count ?? 0;
        public double MontantMensuelTotal => InscriptionsActives?.Sum(i => i.PrixConvenuMensuel) ?? 0;

        // Statistiques de paiements globales
        public int MoisPayesCompletement => EtatPaiements?.Count(ep => ep.EstCompletementPaye) ?? 0;
        public int MoisImpayes => EtatPaiements?.Count(ep => ep.MontantDu > 0 && !ep.EstCompletementPaye && !ep.EstPartiellementPaye) ?? 0;
        public int MoisPartiels => EtatPaiements?.Count(ep => ep.EstPartiellementPaye) ?? 0;

        // Statistiques pour la matière sélectionnée
        public int MoisPayesMatiere => EtatPaiementsMatiere?.Count(ep => ep.EstCompletementPaye) ?? 0;
        public int MoisImpayesMatiere => EtatPaiementsMatiere?.Count(ep => ep.MontantDu > 0 && !ep.EstCompletementPaye && !ep.EstPartiellementPaye) ?? 0;
        public int MoisPartielsMatiere => EtatPaiementsMatiere?.Count(ep => ep.EstPartiellementPaye) ?? 0;

        // Commande pour sélectionner une matière
        public ICommand SelectMatiereCommand => new RelayCommand<Inscription>(ExecuteSelectMatiere);

        // Constructeur
        public EtudiantDetailsViewModel(Etudiant etudiant)
        {
            _inscriptionRepository = new InscriptionRepository();
            _paiementRepository = new PaiementRepository();

            EtudiantActuel = etudiant ?? throw new ArgumentNullException(nameof(etudiant));
            InscriptionsActives = new ObservableCollection<Inscription>();
            EtatPaiements = new ObservableCollection<StatutPaiementMensuel>();

            // Année en cours par défaut
            AnneeSelectionnee = DateTime.Now.Year;

            // Charger les données
            _ = LoadAllDataAsync();
        }

        /// <summary>
        /// Charge toutes les données de l'étudiant
        /// </summary>
        private async Task LoadAllDataAsync()
        {
            StatusMessage = "Chargement des informations...";

            try
            {
                await LoadInscriptionsActivesAsync();
                await LoadEtatPaiementsAsync();
                StatusMessage = "Informations chargées avec succès.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement : {ex.Message}";
            }
        }

        /// <summary>
        /// Charge les inscriptions actives de l'étudiant
        /// </summary>
        private async Task LoadInscriptionsActivesAsync()
        {
            try
            {
                var inscriptions = await _inscriptionRepository.GetActiveInscriptionsForEtudiantAsync(EtudiantActuel.IDEtudiant);
                InscriptionsActives = new ObservableCollection<Inscription>(inscriptions);

                // Notifier les changements des propriétés calculées
                OnPropertyChanged(nameof(NombreCoursSuivis));
                OnPropertyChanged(nameof(MontantMensuelTotal));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement des inscriptions : {ex.Message}";
            }
        }

        /// <summary>
        /// Charge l'état des paiements pour l'année sélectionnée
        /// </summary>
        private async Task LoadEtatPaiementsAsync()
        {
            try
            {
                var statutsPaiements = await _paiementRepository.GetStatutPaiementParMoisAsync(
                    EtudiantActuel.IDEtudiant,
                    AnneeSelectionnee);

                EtatPaiements = new ObservableCollection<StatutPaiementMensuel>(statutsPaiements);

                // Notifier les changements des statistiques
                OnPropertyChanged(nameof(MoisPayesCompletement));
                OnPropertyChanged(nameof(MoisImpayes));
                OnPropertyChanged(nameof(MoisPartiels));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement des paiements : {ex.Message}";
            }
        }

        /// <summary>
        /// Méthode pour charger les paiements d'une matière spécifique
        /// </summary>
        private async Task LoadEtatPaiementsMatiereAsync()
        {
            if (SelectedMatiere == null)
            {
                EtatPaiementsMatiere.Clear();
                return;
            }

            try
            {
                // Tu devras adapter cette méthode dans ton PaiementRepository
                var statutsPaiements = await _paiementRepository.GetStatutPaiementParMoisEtMatiereAsync(
                    EtudiantActuel.IDEtudiant,
                    SelectedMatiere.IDInscription, // ou l'ID de la matière
                    AnneeSelectionnee);

                EtatPaiementsMatiere = new ObservableCollection<StatutPaiementMensuel>(statutsPaiements);

                // Notifier les changements des statistiques
                OnPropertyChanged(nameof(MoisPayesMatiere));
                OnPropertyChanged(nameof(MoisImpayesMatiere));
                OnPropertyChanged(nameof(MoisPartielsMatiere));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement des paiements de la matière : {ex.Message}";
            }
        }

        /// <summary>
        /// Exécute la sélection d'une matière
        /// </summary>
        private void ExecuteSelectMatiere(Inscription inscription)
        {
            SelectedMatiere = inscription;
        }

        /// <summary>
        /// Obtient la liste des années disponibles pour les paiements
        /// </summary>
        public int[] AnneesDisponibles
        {
            get
            {
                var anneeActuelle = DateTime.Now.Year;
                var anneeInscription = anneeActuelle;

                if (DateTime.TryParse(EtudiantActuel?.DateInscriptionSysteme, out DateTime dateInscription))
                {
                    anneeInscription = dateInscription.Year;
                }

                // Retourner les années depuis l'inscription jusqu'à l'année prochaine
                var annees = new List<int>();
                for (int annee = anneeInscription; annee <= anneeActuelle + 1; annee++)
                {
                    annees.Add(annee);
                }

                return annees.OrderByDescending(a => a).ToArray();
            }
        }

        /// <summary>
        /// Actualise les données
        /// </summary>
        public async Task RefreshDataAsync()
        {
            await LoadAllDataAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Classe RelayCommand pour les commandes
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;
        public void Execute(object parameter) => _execute((T)parameter);
        public event EventHandler CanExecuteChanged;
    }

    /// <summary>
    /// Classe d'extension pour les informations supplémentaires d'affichage
    /// </summary>
    public static class EtudiantExtensions
    {
        public static string GetStatutColor(this Etudiant etudiant)
        {
            return etudiant.EstArchive ? "#f56565" : "#48bb78";
        }

        public static string GetStatutText(this Etudiant etudiant)
        {
            return etudiant.EstArchive ? "Archivé" : "Actif";
        }

        public static string GetStatutIcon(this Etudiant etudiant)
        {
            return etudiant.EstArchive ? "📁" : "✅";
        }
        
    }
}