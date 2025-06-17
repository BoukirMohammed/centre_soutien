using centre_soutien.Models;      // Pour la classe Salle
using centre_soutien.DataAccess;  // Pour SalleRepository
using System;                     // Pour ArgumentNullException, Exception, EventHandler
using System.Collections.ObjectModel; // Pour ObservableCollection
using System.ComponentModel;          // Pour INotifyPropertyChanged
using System.Runtime.CompilerServices; // Pour CallerMemberName
using System.Threading.Tasks;         // Pour Task
using System.Windows.Input;           // Pour ICommand
using System.Linq;
using centre_soutien.Commands; // Pour des opérations LINQ sur la collection si besoin
using centre_soutien.DataAccess.Repositories;
using centre_soutien.Services;
using centre_soutien.Helpers;

namespace centre_soutien.ViewModels {// Ajuste le namespace

    // Classe de base pour ICommand (RelayCommand)
    // Tu peux mettre cette classe dans un dossier Helpers/ ou Commands/
    // Si tu l'as déjà créée ailleurs, tu peux supprimer cette implémentation ici.
    public class SalleViewModel : INotifyPropertyChanged
    {

        public bool CanUserArchive => CurrentUserSession.IsAdmin; // Ou CurrentUserSession.IsAdmin || CurrentUserSession.IsSecretaire si la secrétaire peut aussi archiver certains types.

        private readonly SalleRepository _salleRepository;

        private ObservableCollection<Salle> _salles;
        public ObservableCollection<Salle> Salles
        {
            get => _salles;
            set { _salles = value; OnPropertyChanged(); }
        }

        private Salle _selectedSalle;
        public Salle SelectedSalle
        {
            get => _selectedSalle;
            set
            {
                _selectedSalle = value;
                OnPropertyChanged();
                // Mettre à jour les champs de saisie si une salle est sélectionnée
                if (_selectedSalle != null)
                {
                    NomSalleInput = _selectedSalle.NomOuNumeroSalle;
                    CapaciteInput = _selectedSalle.Capacite;
                    DescriptionInput = _selectedSalle.Description;
                }
                else
                {
                    ClearInputFields(); // Efface les champs si rien n'est sélectionné
                }
                // Informer les commandes que leur état CanExecute pourrait avoir changé
                (AddSalleCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateSalleCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ArchiveSalleCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Propriétés pour les champs de saisie du formulaire
        private string _nomSalleInput;
        public string NomSalleInput
        {
            get => _nomSalleInput;
            set { _nomSalleInput = value; OnPropertyChanged(); (AddSalleCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private int? _capaciteInput;
        public int? CapaciteInput
        {
            get => _capaciteInput;
            set { _capaciteInput = value; OnPropertyChanged(); }
        }

        private string _descriptionInput;
        public string DescriptionInput
        {
            get => _descriptionInput;
            set { _descriptionInput = value; OnPropertyChanged(); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        // Commandes
        public ICommand LoadSallesCommand { get; }
        public ICommand AddSalleCommand { get; }
        public ICommand UpdateSalleCommand { get; }
        public ICommand ArchiveSalleCommand { get; }
        public ICommand ClearFormCommand { get; }


        public SalleViewModel()
        {
            _salleRepository = new SalleRepository(); // Instance du Repository
            Salles = new ObservableCollection<Salle>();

            // Initialisation des commandes
            LoadSallesCommand = new RelayCommand(async param => await LoadSallesAsync());
            AddSalleCommand = new RelayCommand(async param => await AddSalleAsync(), CanAddOrUpdateSalle); // CanExecute partagé
            UpdateSalleCommand = new RelayCommand(async param => await UpdateSalleAsync(), CanUpdateOrArchiveSalle);
            ArchiveSalleCommand = new RelayCommand(
                async param => await ArchiveSalleAsync(),
                param => SelectedSalle != null && CanUserArchive // Le bouton ne s'active que si un étudiant est sélectionné ET l'utilisateur a le droit
            );
            ClearFormCommand = new RelayCommand(param => ClearInputFieldsAndSelection());

            
            // Charger les salles initialement au démarrage du ViewModel
            _ = LoadSallesAsync(); // Lancer en arrière-plan sans attendre ici (fire and forget)
        }

        public async Task LoadSallesAsync()
        {
            try
            {
                var sallesList = await _salleRepository.GetAllSallesAsync();
                Salles = new ObservableCollection<Salle>(sallesList); // Met à jour la collection liée à l'UI
                StatusMessage = $"Chargement de {Salles.Count} salles réussi.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors du chargement des salles: {ex.Message}";
                // Gérer l'erreur (par exemple, logger, afficher un message plus formel à l'utilisateur)
            }
        }

        private bool CanAddOrUpdateSalle(object parameter)
        {
            // On peut ajouter ou modifier une salle si le nom n'est pas vide (exemple simple de validation)
            return !string.IsNullOrWhiteSpace(NomSalleInput);
        }

        private async Task AddSalleAsync()
        {
            if (!CanAddOrUpdateSalle(null)) return;

            var nouvelleSalle = new Salle
            {
                NomOuNumeroSalle = NomSalleInput,
                Capacite = CapaciteInput,
                Description = DescriptionInput,
                EstArchivee = false // Une nouvelle salle n'est pas archivée par défaut
            };

            try
            {
                await _salleRepository.AddSalleAsync(nouvelleSalle);
                await LoadSallesAsync(); // Recharger la liste pour voir la nouvelle salle
                ClearInputFieldsAndSelection(); // Vider le formulaire
                StatusMessage = $"Salle '{nouvelleSalle.NomOuNumeroSalle}' ajoutée avec succès.";
            }
            catch (InvalidOperationException ex) // Spécifiquement pour les erreurs d'unicité du nom
            {
                StatusMessage = ex.Message;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors de l'ajout de la salle: {ex.Message}";
            }
        }

        private bool CanUpdateOrArchiveSalle(object parameter)
        {
            // On peut modifier ou archiver seulement si une salle est sélectionnée dans la liste
            // Et si les champs obligatoires pour la mise à jour sont valides (si applicable)
            return SelectedSalle != null && !string.IsNullOrWhiteSpace(NomSalleInput);
        }

        private async Task UpdateSalleAsync()
        {
            if (!CanUpdateOrArchiveSalle(null) || SelectedSalle == null) return;

            // Créer un objet avec les valeurs mises à jour, en conservant l'ID original
            var salleAMettreAJour = new Salle
            {
                IDSalle = SelectedSalle.IDSalle, // Crucial de garder l'ID
                NomOuNumeroSalle = NomSalleInput,
                Capacite = CapaciteInput,
                Description = DescriptionInput,
                EstArchivee = SelectedSalle.EstArchivee // Conserver l'état d'archivage actuel lors d'une simple mise à jour des données
            };

            try
            {
                await _salleRepository.UpdateSalleAsync(salleAMettreAJour);
                await LoadSallesAsync(); // Recharger la liste pour voir les modifications
                ClearInputFieldsAndSelection(); // Vider le formulaire
                StatusMessage = $"Salle '{salleAMettreAJour.NomOuNumeroSalle}' mise à jour avec succès.";
            }
            catch (InvalidOperationException ex)  // Spécifiquement pour les erreurs d'unicité du nom
            {
                StatusMessage = ex.Message;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors de la mise à jour de la salle: {ex.Message}";
            }
        }

        private async Task ArchiveSalleAsync()
        {
            if (!CanUpdateOrArchiveSalle(null) || SelectedSalle == null) return;

            // Il serait bon d'avoir une confirmation de l'utilisateur ici (via un service de dialogue)
            // Exemple : bool confirmation = await _dialogService.ShowConfirmationAsync("Archiver la salle ?", $"Voulez-vous vraiment archiver la salle '{SelectedSalle.NomOuNumeroSalle}' ?");
            // if (!confirmation) return;

            try
            {
                await _salleRepository.ArchiveSalleAsync(SelectedSalle.IDSalle);
                await LoadSallesAsync(); // Recharger la liste (la salle archivée ne devrait plus apparaître si GetAllSallesAsync filtre sur EstArchivee=false)
                ClearInputFieldsAndSelection(); // Vider le formulaire
                StatusMessage = $"Salle '{SelectedSalle.NomOuNumeroSalle}' archivée avec succès.";
            }
            catch (InvalidOperationException ex) // Si l'archivage est empêché (ex: salle utilisée)
            {
                 StatusMessage = ex.Message;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors de l'archivage de la salle: {ex.Message}";
            }
        }
        
        private void ClearInputFields()
        {
            NomSalleInput = string.Empty;
            CapaciteInput = null; // Remettre à null pour un int?
            DescriptionInput = string.Empty;
            // Ne pas effacer StatusMessage ici, car il peut contenir le résultat d'une action
        }

        private void ClearInputFieldsAndSelection()
        {
            ClearInputFields();
            SelectedSalle = null; // Désélectionner la salle dans la liste
            StatusMessage = string.Empty; // Effacer le message de statut ici
        }


        // Implémentation de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Méthode pour forcer la réévaluation de CanExecute pour les commandes RelayCommand
        // Utile si tu as besoin de le faire explicitement depuis une autre partie du code
        // Note: L'implémentation de RelayCommand utilise CommandManager.RequerySuggested,
        // ce qui est souvent suffisant.
        public void RaiseCanExecuteChangedForAllCommands()
        {
            (LoadSallesCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (AddSalleCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (UpdateSalleCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ArchiveSalleCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ClearFormCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    // Extension pour RelayCommand pour exposer RaiseCanExecuteChanged publiquement
    public static class RelayCommandExtensions
    {
        public static void RaiseCanExecuteChanged(this RelayCommand command)
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}