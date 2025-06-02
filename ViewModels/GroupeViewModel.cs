using centre_soutien.Models;
using centre_soutien.DataAccess;
using centre_soutien.Commands; // Assure-toi que ce using pointe vers ta classe RelayCommand
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq; // Pour .FirstOrDefault() par exemple
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic; // Pour List<T>

namespace centre_soutien.ViewModels
{
    public class GroupeViewModel : INotifyPropertyChanged
    {
        private readonly GroupeRepository _groupeRepository;
        private readonly MatiereRepository _matiereRepository; // Pour charger les matières
        private readonly ProfesseurRepository _professeurRepository; // Pour charger les professeurs

        // Collection pour afficher les groupes dans le DataGrid
        private ObservableCollection<Groupe> _groupes;
        public ObservableCollection<Groupe> Groupes
        {
            get => _groupes;
            set { _groupes = value; OnPropertyChanged(); }
        }

        private Groupe? _selectedGroupe;
        public Groupe? SelectedGroupe
        {
            get => _selectedGroupe;
            set
            {
                _selectedGroupe = value;
                OnPropertyChanged();
                if (_selectedGroupe != null)
                {
                    NomDescriptifInput = _selectedGroupe.NomDescriptifGroupe;
                    SelectedMatiereForForm = AllMatieres.FirstOrDefault(m => m.IDMatiere == _selectedGroupe.IDMatiere);
                    SelectedProfesseurForForm = AllProfesseurs.FirstOrDefault(p => p.IDProfesseur == _selectedGroupe.IDProfesseur);
                    NiveauInput = _selectedGroupe.Niveau;
                    NotesInput = _selectedGroupe.Notes;
                }
                else
                {
                    ClearInputFields();
                }
                (AddGroupeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateGroupeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ArchiveGroupeCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // --- Collections pour les ComboBox du formulaire ---
        private ObservableCollection<Matiere> _allMatieres;
        public ObservableCollection<Matiere> AllMatieres
        {
            get => _allMatieres;
            set { _allMatieres = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Professeur> _allProfesseurs;
        public ObservableCollection<Professeur> AllProfesseurs
        {
            get => _allProfesseurs;
            set { _allProfesseurs = value; OnPropertyChanged(); }
        }

        // --- Propriétés pour les champs de saisie du formulaire ---
        private string _nomDescriptifInput = string.Empty;
        public string NomDescriptifInput
        {
            get => _nomDescriptifInput;
            set { _nomDescriptifInput = value; OnPropertyChanged(); (AddGroupeCommand as RelayCommand)?.RaiseCanExecuteChanged(); (UpdateGroupeCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private Matiere? _selectedMatiereForForm; // Matière sélectionnée dans la ComboBox
        public Matiere? SelectedMatiereForForm
        {
            get => _selectedMatiereForForm;
            set { _selectedMatiereForForm = value; OnPropertyChanged(); (AddGroupeCommand as RelayCommand)?.RaiseCanExecuteChanged(); (UpdateGroupeCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private Professeur? _selectedProfesseurForForm; // Professeur sélectionné dans la ComboBox
        public Professeur? SelectedProfesseurForForm
        {
            get => _selectedProfesseurForForm;
            set { _selectedProfesseurForForm = value; OnPropertyChanged(); (AddGroupeCommand as RelayCommand)?.RaiseCanExecuteChanged(); (UpdateGroupeCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private string? _niveauInput;
        public string? NiveauInput
        {
            get => _niveauInput;
            set { _niveauInput = value; OnPropertyChanged(); }
        }

        private string? _notesInput;
        public string? NotesInput
        {
            get => _notesInput;
            set { _notesInput = value; OnPropertyChanged(); }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        // Commandes
        public ICommand LoadDataCommand { get; } // Commande pour charger tous les types de données
        public ICommand AddGroupeCommand { get; }
        public ICommand UpdateGroupeCommand { get; }
        public ICommand ArchiveGroupeCommand { get; }
        public ICommand ClearFormCommand { get; }

        public GroupeViewModel()
        {
            _groupeRepository = new GroupeRepository();
            _matiereRepository = new MatiereRepository();
            _professeurRepository = new ProfesseurRepository();

            Groupes = new ObservableCollection<Groupe>();
            AllMatieres = new ObservableCollection<Matiere>();
            AllProfesseurs = new ObservableCollection<Professeur>();

            LoadDataCommand = new RelayCommand(async param => await LoadAllDataAsync());
            AddGroupeCommand = new RelayCommand(async param => await AddGroupeAsync(), CanAddOrUpdateGroupe);
            UpdateGroupeCommand = new RelayCommand(async param => await UpdateGroupeAsync(), CanUpdateOrArchiveGroupe);
            ArchiveGroupeCommand = new RelayCommand(async param => await ArchiveGroupeAsync(), CanArchiveGroupe);
            ClearFormCommand = new RelayCommand(param => ClearInputFieldsAndSelection());

            _ = LoadAllDataAsync(); // Charger toutes les données nécessaires au démarrage
        }

        public async Task LoadAllDataAsync()
        {
            await LoadGroupesAsync();
            await LoadMatieresForComboBoxAsync();
            await LoadProfesseursForComboBoxAsync();
        }

        private async Task LoadGroupesAsync()
        {
            try
            {
                var groupesList = await _groupeRepository.GetAllGroupesWithDetailsAsync(); // Avec détails pour affichage
                Groupes = new ObservableCollection<Groupe>(groupesList);
                StatusMessage = $"Chargement de {Groupes.Count} groupes réussi.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur chargement groupes: {ex.Message}";
            }
        }

        private async Task LoadMatieresForComboBoxAsync()
        {
            try
            {
                var matieresList = await _matiereRepository.GetAllMatieresAsync(); // Que les matières actives
                AllMatieres = new ObservableCollection<Matiere>(matieresList);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur chargement matières pour formulaire: {ex.Message}";
            }
        }

        private async Task LoadProfesseursForComboBoxAsync()
        {
            try
            {
                var professeursList = await _professeurRepository.GetAllProfesseursAsync(); // Que les profs actifs
                AllProfesseurs = new ObservableCollection<Professeur>(professeursList);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur chargement professeurs pour formulaire: {ex.Message}";
            }
        }

        private bool CanAddOrUpdateGroupe(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(NomDescriptifInput) &&
                   SelectedMatiereForForm != null &&
                   SelectedProfesseurForForm != null;
        }

        private async Task AddGroupeAsync()
        {
            if (!CanAddOrUpdateGroupe(null))
            {
                StatusMessage = "Nom du groupe, matière et professeur sont requis.";
                return;
            }

            var nouveauGroupe = new Groupe
            {
                NomDescriptifGroupe = NomDescriptifInput,
                IDMatiere = SelectedMatiereForForm!.IDMatiere, // ! car vérifié dans CanAddOrUpdateGroupe
                IDProfesseur = SelectedProfesseurForForm!.IDProfesseur, // ! car vérifié
                Niveau = NiveauInput,
                Notes = NotesInput,
                EstArchive = false
            };

            try
            {
                await _groupeRepository.AddGroupeAsync(nouveauGroupe);
                await LoadGroupesAsync();
                ClearInputFieldsAndSelection();
                StatusMessage = $"Groupe '{nouveauGroupe.NomDescriptifGroupe}' ajouté.";
            }
            catch (InvalidOperationException ex) { StatusMessage = ex.Message; }
            catch (Exception ex) { StatusMessage = $"Erreur ajout groupe: {ex.Message}"; }
        }
        
        private bool CanUpdateOrArchiveGroupe(object? parameter)
        {
            return SelectedGroupe != null &&
                   !string.IsNullOrWhiteSpace(NomDescriptifInput) &&
                   SelectedMatiereForForm != null &&
                   SelectedProfesseurForForm != null;
        }
        private bool CanArchiveGroupe(object? parameter)
        {
            return SelectedGroupe != null;
        }

        private async Task UpdateGroupeAsync()
        {
            if (!CanUpdateOrArchiveGroupe(null) || SelectedGroupe == null) return;

            var groupeAMettreAJour = new Groupe
            {
                IDGroupe = SelectedGroupe.IDGroupe,
                NomDescriptifGroupe = NomDescriptifInput,
                IDMatiere = SelectedMatiereForForm!.IDMatiere,
                IDProfesseur = SelectedProfesseurForForm!.IDProfesseur,
                Niveau = NiveauInput,
                Notes = NotesInput,
                EstArchive = SelectedGroupe.EstArchive
            };

            try
            {
                await _groupeRepository.UpdateGroupeAsync(groupeAMettreAJour);
                await LoadGroupesAsync();
                ClearInputFieldsAndSelection();
                StatusMessage = $"Groupe '{groupeAMettreAJour.NomDescriptifGroupe}' mis à jour.";
            }
            catch (InvalidOperationException ex) { StatusMessage = ex.Message; }
            catch (Exception ex) { StatusMessage = $"Erreur MàJ groupe: {ex.Message}"; }
        }

        private async Task ArchiveGroupeAsync()
        {
            if (!CanArchiveGroupe(null) || SelectedGroupe == null) return;

            try
            {
                await _groupeRepository.ArchiveGroupeAsync(SelectedGroupe.IDGroupe);
                await LoadGroupesAsync();
                ClearInputFieldsAndSelection();
                StatusMessage = $"Groupe '{SelectedGroupe.NomDescriptifGroupe}' archivé.";
            }
            catch (InvalidOperationException ex) { StatusMessage = ex.Message; }
            catch (Exception ex) { StatusMessage = $"Erreur archivage groupe: {ex.Message}"; }
        }

        private void ClearInputFields()
        {
            NomDescriptifInput = string.Empty;
            SelectedMatiereForForm = null;
            SelectedProfesseurForForm = null;
            NiveauInput = null;
            NotesInput = null;
        }

        private void ClearInputFieldsAndSelection()
        {
            ClearInputFields();
            SelectedGroupe = null;
            StatusMessage = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}