using centre_soutien.Models;
using centre_soutien.DataAccess;
using centre_soutien.Commands; // Pour RelayCommand
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Globalization; // Pour CultureInfo
using System.Collections.Generic; // Pour List et IEnumerable
using System.Linq;
using centre_soutien.DataAccess.Repositories; // Pour Enum.GetValues et FirstOrDefault

namespace centre_soutien.ViewModels
{
    public class PlanningViewModel : INotifyPropertyChanged
    {
        private readonly SeancePlanningRepository _seanceRepository;
        private readonly GroupeRepository _groupeRepository;
        private readonly SalleRepository _salleRepository;

        // --- Collections pour les ComboBox du formulaire d'ajout ---
        private ObservableCollection<Groupe> _allGroupesActifs;
        public ObservableCollection<Groupe> AllGroupesActifs
        {
            get => _allGroupesActifs;
            set { _allGroupesActifs = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Salle> _allSallesActives;
        public ObservableCollection<Salle> AllSallesActives
        {
            get => _allSallesActives;
            set { _allSallesActives = value; OnPropertyChanged(); }
        }
        
        // Pour la ComboBox des jours de la semaine
// Dans ViewModels/PlanningViewModel.cs

// Pour la ComboBox des jours de la semaine, avec Lundi en premier
        public IEnumerable<DayOfWeek> JoursDeLaSemaine => 
            Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .OrderBy(d => ((int)d + 6) % 7); // Convertir d en int avant les opérations

        // --- Propriétés pour le formulaire d'ajout de CRÉNEAU RÉCURRENT ---
        private Groupe? _selectedGroupeForNewCreneau;
        public Groupe? SelectedGroupeForNewCreneau
        {
            get => _selectedGroupeForNewCreneau;
            set { _selectedGroupeForNewCreneau = value; OnPropertyChanged(); (AddCreneauCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private Salle? _selectedSalleForNewCreneau;
        public Salle? SelectedSalleForNewCreneau
        {
            get => _selectedSalleForNewCreneau;
            set { _selectedSalleForNewCreneau = value; OnPropertyChanged(); (AddCreneauCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private DayOfWeek _selectedJourSemaineForNewCreneau = DayOfWeek.Monday;
        public DayOfWeek SelectedJourSemaineForNewCreneau
        {
            get => _selectedJourSemaineForNewCreneau;
            set { _selectedJourSemaineForNewCreneau = value; OnPropertyChanged(); (AddCreneauCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private DateTime? _dateDebutValiditeInput = DateTime.Today;
        public DateTime? DateDebutValiditeInput
        {
            get => _dateDebutValiditeInput;
            set { _dateDebutValiditeInput = value; OnPropertyChanged(); (AddCreneauCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private DateTime? _dateFinValiditeInput; 
        public DateTime? DateFinValiditeInput
        {
            get => _dateFinValiditeInput;
            set { _dateFinValiditeInput = value; OnPropertyChanged(); (AddCreneauCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }
        
        private string _heureDebutInput = "09:00"; 
        public string HeureDebutInput
        {
            get => _heureDebutInput;
            set { _heureDebutInput = value; OnPropertyChanged(); (AddCreneauCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private string _heureFinInput = "10:00";
        public string HeureFinInput
        {
            get => _heureFinInput;
            set { _heureFinInput = value; OnPropertyChanged(); (AddCreneauCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private string? _notesNewCreneauInput;
        public string? NotesNewCreneauInput
        {
            get => _notesNewCreneauInput;
            set { _notesNewCreneauInput = value; OnPropertyChanged(); }
        }

        private ObservableCollection<SeancePlanning> _creneauxPlanifies;
        public ObservableCollection<SeancePlanning> CreneauxPlanifies
        {
            get => _creneauxPlanifies;
            set { _creneauxPlanifies = value; OnPropertyChanged(); }
        }
        
        private SeancePlanning? _selectedCreneauForEditDelete;
        public SeancePlanning? SelectedCreneauForEditDelete
        {
            get => _selectedCreneauForEditDelete;
            set 
            { 
                _selectedCreneauForEditDelete = value; 
                OnPropertyChanged(); 
                (DeactivateCreneauCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ActivateCreneauCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteCreneauCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateCreneauCommand as RelayCommand)?.RaiseCanExecuteChanged(); // Pour le bouton Modifier
                
                if (_selectedCreneauForEditDelete != null)
                {
                    SelectedGroupeForNewCreneau = AllGroupesActifs.FirstOrDefault(g => g.IDGroupe == _selectedCreneauForEditDelete.IDGroupe);
                    SelectedSalleForNewCreneau = AllSallesActives.FirstOrDefault(s => s.IDSalle == _selectedCreneauForEditDelete.IDSalle);
                    SelectedJourSemaineForNewCreneau = _selectedCreneauForEditDelete.JourSemaine;
                    HeureDebutInput = _selectedCreneauForEditDelete.HeureDebut;
                    HeureFinInput = _selectedCreneauForEditDelete.HeureFin;
                    DateDebutValiditeInput = DateTime.ParseExact(_selectedCreneauForEditDelete.DateDebutValidite, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    DateFinValiditeInput = !string.IsNullOrEmpty(_selectedCreneauForEditDelete.DateFinValidite) ? DateTime.ParseExact(_selectedCreneauForEditDelete.DateFinValidite, "yyyy-MM-dd", CultureInfo.InvariantCulture) : (DateTime?)null;
                    NotesNewCreneauInput = _selectedCreneauForEditDelete.Notes;
                }
                 else
                {
                    // Optionnel: vider le formulaire si rien n'est sélectionné après une action.
                    // ClearNewCreneauForm(); // Ou juste laisser les dernières valeurs saisies pour un nouvel ajout
                }
            }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoadInitialDataCommand { get; }
        public ICommand AddCreneauCommand { get; }
        public ICommand UpdateCreneauCommand { get; }
        public ICommand DeactivateCreneauCommand { get; }
        public ICommand ActivateCreneauCommand { get; }
        public ICommand DeleteCreneauCommand { get; } 
        public ICommand ClearFormCommand { get; }

        public PlanningViewModel()
        {
            _seanceRepository = new SeancePlanningRepository();
            _groupeRepository = new GroupeRepository();
            _salleRepository = new SalleRepository();

            AllGroupesActifs = new ObservableCollection<Groupe>();
            AllSallesActives = new ObservableCollection<Salle>();
            CreneauxPlanifies = new ObservableCollection<SeancePlanning>();

            LoadInitialDataCommand = new RelayCommand(async param => await LoadAllInitialDataAsync());
            AddCreneauCommand = new RelayCommand(async param => await AddCreneauAsync(), CanAddOrUpdateCreneau);
            UpdateCreneauCommand = new RelayCommand(async param => await UpdateCreneauAsync(), CanUpdateCreneau);
            DeactivateCreneauCommand = new RelayCommand(async param => await DeactivateCreneauAsync(), CanDeactivateCreneau);
            ActivateCreneauCommand = new RelayCommand(async param => await ActivateCreneauAsync(), CanActivateCreneau);
            DeleteCreneauCommand = new RelayCommand(async param => await DeleteCreneauAsync(), CanDeleteCreneau);
            ClearFormCommand = new RelayCommand(param => ClearNewCreneauFormAndSelection());

            _ = LoadAllInitialDataAsync();
        }

        public async Task LoadAllInitialDataAsync()
        {
            await LoadGroupesForComboBoxAsync();
            await LoadSallesForComboBoxAsync();
            await LoadCreneauxPlanifiesAsync(); 
        }

        private async Task LoadGroupesForComboBoxAsync()
        {
            try
            {
                var groupesList = await _groupeRepository.GetAllGroupesWithDetailsAsync();
                AllGroupesActifs = new ObservableCollection<Groupe>(groupesList.Where(g => !g.EstArchive)); // Filtrer les groupes actifs
            }
            catch (Exception ex) { StatusMessage = $"Erreur chargement groupes: {ex.Message}"; }
        }

        private async Task LoadSallesForComboBoxAsync()
        {
            try
            {
                var sallesList = await _salleRepository.GetAllSallesAsync();
                AllSallesActives = new ObservableCollection<Salle>(sallesList.Where(s => !s.EstArchivee)); // Filtrer les salles actives
            }
            catch (Exception ex) { StatusMessage = $"Erreur chargement salles: {ex.Message}"; }
        }

        private async Task LoadCreneauxPlanifiesAsync()
        {
            try
            {
                var seancesList = await _seanceRepository.GetAllActiveSeancesWithDetailsAsync();
                CreneauxPlanifies = new ObservableCollection<SeancePlanning>(seancesList);
                StatusMessage = $"Chargement de {CreneauxPlanifies.Count} créneaux planifiés réussi.";
            }
            catch (Exception ex) { StatusMessage = $"Erreur chargement créneaux: {ex.Message}"; }
        }

        private bool CanAddOrUpdateCreneau(object? parameter)
        {
            return SelectedGroupeForNewCreneau != null &&
                   SelectedSalleForNewCreneau != null &&
                   DateDebutValiditeInput.HasValue &&
                   !string.IsNullOrWhiteSpace(HeureDebutInput) &&
                   !string.IsNullOrWhiteSpace(HeureFinInput);
        }

        private async Task AddCreneauAsync()
        {
            if (!CanAddOrUpdateCreneau(null))
            {
                StatusMessage = "Groupe, Salle, Jour, Date Début, et Heures sont requis.";
                return;
            }
            try
            {
                _seanceRepository.ValidateSeanceTimes(HeureDebutInput, HeureFinInput);
                _seanceRepository.ValidateDateRange(DateDebutValiditeInput!.Value.ToString("yyyy-MM-dd"), DateFinValiditeInput?.ToString("yyyy-MM-dd"));
            }
            catch(ArgumentException ex) { StatusMessage = ex.Message; return; }
            catch(FormatException ex) { StatusMessage = ex.Message; return; }

            var nouveauCreneau = new SeancePlanning
            {
                IDGroupe = SelectedGroupeForNewCreneau!.IDGroupe,
                IDSalle = SelectedSalleForNewCreneau!.IDSalle,
                JourSemaine = SelectedJourSemaineForNewCreneau,
                HeureDebut = HeureDebutInput,
                HeureFin = HeureFinInput,
                DateDebutValidite = DateDebutValiditeInput!.Value.ToString("yyyy-MM-dd"),
                DateFinValidite = DateFinValiditeInput?.ToString("yyyy-MM-dd"),
                Notes = NotesNewCreneauInput,
                EstActif = true
            };
            try
            {
                await _seanceRepository.AddSeanceAsync(nouveauCreneau);
                await LoadCreneauxPlanifiesAsync();
                StatusMessage = $"Créneau ajouté: {nouveauCreneau.JourSemaine} de {nouveauCreneau.HeureDebut} à {nouveauCreneau.HeureFin}.";
                ClearNewCreneauForm(); 
            }
            catch (InvalidOperationException ex) { StatusMessage = ex.Message; }
            catch (Exception ex) { StatusMessage = $"Erreur ajout créneau: {ex.Message}"; }
        }
        
        private bool CanUpdateCreneau(object? parameter) => SelectedCreneauForEditDelete != null && CanAddOrUpdateCreneau(null);
        private async Task UpdateCreneauAsync()
        {
            if (!CanUpdateCreneau(null) || SelectedCreneauForEditDelete == null) return;
            try
            {
                _seanceRepository.ValidateSeanceTimes(HeureDebutInput, HeureFinInput);
                _seanceRepository.ValidateDateRange(DateDebutValiditeInput!.Value.ToString("yyyy-MM-dd"), DateFinValiditeInput?.ToString("yyyy-MM-dd"));
            }
            catch(ArgumentException ex) { StatusMessage = ex.Message; return; }
            catch(FormatException ex) { StatusMessage = ex.Message; return; }

            var creneauAMettreAJour = new SeancePlanning
            {
                IDSeance = SelectedCreneauForEditDelete.IDSeance,
                IDGroupe = SelectedGroupeForNewCreneau!.IDGroupe,
                IDSalle = SelectedSalleForNewCreneau!.IDSalle,
                JourSemaine = SelectedJourSemaineForNewCreneau,
                HeureDebut = HeureDebutInput,
                HeureFin = HeureFinInput,
                DateDebutValidite = DateDebutValiditeInput!.Value.ToString("yyyy-MM-dd"),
                DateFinValidite = DateFinValiditeInput?.ToString("yyyy-MM-dd"),
                Notes = NotesNewCreneauInput,
                EstActif = SelectedCreneauForEditDelete.EstActif 
            };
            try
            {
                await _seanceRepository.UpdateSeanceAsync(creneauAMettreAJour);
                await LoadCreneauxPlanifiesAsync();
                StatusMessage = $"Créneau ID {creneauAMettreAJour.IDSeance} mis à jour.";
                ClearNewCreneauFormAndSelection();
            }
            catch (InvalidOperationException ex) { StatusMessage = ex.Message; }
            catch (Exception ex) { StatusMessage = $"Erreur MàJ créneau: {ex.Message}"; }
        }

        private bool CanDeactivateCreneau(object? parameter) => SelectedCreneauForEditDelete != null && SelectedCreneauForEditDelete.EstActif;
        private async Task DeactivateCreneauAsync()
        {
            if (!CanDeactivateCreneau(null) || SelectedCreneauForEditDelete == null ) return;
            try
            {
                await _seanceRepository.DeactivateSeanceAsync(SelectedCreneauForEditDelete.IDSeance);
                await LoadCreneauxPlanifiesAsync();
                StatusMessage = $"Créneau ID {SelectedCreneauForEditDelete.IDSeance} désactivé.";
                ClearNewCreneauFormAndSelection();
            }
            catch (Exception ex) { StatusMessage = $"Erreur désactivation: {ex.Message}"; }
        }

        private bool CanActivateCreneau(object? parameter) => SelectedCreneauForEditDelete != null && !SelectedCreneauForEditDelete.EstActif;
        private async Task ActivateCreneauAsync()
        {
            if (!CanActivateCreneau(null) || SelectedCreneauForEditDelete == null) return;
            try
            {
                await _seanceRepository.ActivateSeanceAsync(SelectedCreneauForEditDelete.IDSeance);
                await LoadCreneauxPlanifiesAsync();
                StatusMessage = $"Créneau ID {SelectedCreneauForEditDelete.IDSeance} réactivé.";
                ClearNewCreneauFormAndSelection();
            }
            catch (Exception ex) { StatusMessage = $"Erreur réactivation: {ex.Message}"; }
        }
        
        private bool CanDeleteCreneau(object? parameter) => SelectedCreneauForEditDelete != null;
        private async Task DeleteCreneauAsync()
        {
            if (!CanDeleteCreneau(null) || SelectedCreneauForEditDelete == null) return;
            // TODO: Confirmation utilisateur
            try
            {
                await _seanceRepository.DeleteSeanceAsync(SelectedCreneauForEditDelete.IDSeance);
                await LoadCreneauxPlanifiesAsync();
                StatusMessage = $"Créneau ID {SelectedCreneauForEditDelete.IDSeance} supprimé.";
                ClearNewCreneauFormAndSelection();
            }
            catch (Exception ex) { StatusMessage = $"Erreur suppression: {ex.Message}"; }
        }

        private void ClearNewCreneauForm()
        {
            SelectedGroupeForNewCreneau = null;
            SelectedSalleForNewCreneau = null;
            SelectedJourSemaineForNewCreneau = DayOfWeek.Monday;
            DateDebutValiditeInput = DateTime.Today;
            DateFinValiditeInput = null;
            HeureDebutInput = "09:00";
            HeureFinInput = "10:00";
            NotesNewCreneauInput = string.Empty;
        }
        private void ClearNewCreneauFormAndSelection()
        {
            ClearNewCreneauForm();
            SelectedCreneauForEditDelete = null; // Désélectionner aussi
            StatusMessage = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}