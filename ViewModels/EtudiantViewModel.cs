using centre_soutien.Models;
using centre_soutien.DataAccess;
using centre_soutien.Commands; // Assure-toi que ce using pointe vers ta classe RelayCommand
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using centre_soutien.Services; // Pour CurrentUserSession


namespace centre_soutien.ViewModels
{
    public class EtudiantViewModel : INotifyPropertyChanged
    {
        private readonly EtudiantRepository _etudiantRepository;

        private ObservableCollection<Etudiant> _etudiants;
        public ObservableCollection<Etudiant> Etudiants
        {
            get => _etudiants;
            set { _etudiants = value; OnPropertyChanged(); }
        }

        private Etudiant? _selectedEtudiant; // Nullable
        public bool CanUserArchive => CurrentUserSession.IsAdmin;
        
        public Etudiant? SelectedEtudiant
        {
            get => _selectedEtudiant;
            set
            {
                _selectedEtudiant = value;
                OnPropertyChanged();
                if (_selectedEtudiant != null)
                {
                    NomInput = _selectedEtudiant.Nom;
                    PrenomInput = _selectedEtudiant.Prenom;
                    DateNaissanceInput = _selectedEtudiant.DateNaissance; // Peut nécessiter une conversion si DateTime
                    AdresseInput = _selectedEtudiant.Adresse;
                    TelephoneInput = _selectedEtudiant.Telephone;
                    LyceeInput = _selectedEtudiant.Lycee;
                    NotesInput = _selectedEtudiant.Notes;
                }
                else
                {
                    ClearInputFields();
                }
                (AddEtudiantCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateEtudiantCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ArchiveEtudiantCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Champs pour le formulaire
        private string _nomInput = string.Empty;
        public string NomInput
        {
            get => _nomInput;
            set { _nomInput = value; OnPropertyChanged(); (AddEtudiantCommand as RelayCommand)?.RaiseCanExecuteChanged(); (UpdateEtudiantCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private string _prenomInput = string.Empty;
        public string PrenomInput
        {
            get => _prenomInput;
            set { _prenomInput = value; OnPropertyChanged(); (AddEtudiantCommand as RelayCommand)?.RaiseCanExecuteChanged(); (UpdateEtudiantCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private string? _dateNaissanceInput; // Format "YYYY-MM-DD"
        public string? DateNaissanceInput
        {
            get => _dateNaissanceInput;
            set { _dateNaissanceInput = value; OnPropertyChanged(); }
        }

        private string? _adresseInput;
        public string? AdresseInput
        {
            get => _adresseInput;
            set { _adresseInput = value; OnPropertyChanged(); }
        }

        private string? _telephoneInput;
        public string? TelephoneInput
        {
            get => _telephoneInput;
            set { _telephoneInput = value; OnPropertyChanged(); }
        }

        private string? _lyceeInput;
        public string? LyceeInput
        {
            get => _lyceeInput;
            set { _lyceeInput = value; OnPropertyChanged(); }
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
        public ICommand LoadEtudiantsCommand { get; }
        public ICommand AddEtudiantCommand { get; }
        public ICommand UpdateEtudiantCommand { get; }
        public ICommand ArchiveEtudiantCommand { get; }
        public ICommand ClearFormCommand { get; }

        public EtudiantViewModel()
        {
            _etudiantRepository = new EtudiantRepository();
            Etudiants = new ObservableCollection<Etudiant>();

            LoadEtudiantsCommand = new RelayCommand(async param => await LoadEtudiantsAsync());
            AddEtudiantCommand = new RelayCommand(async param => await AddEtudiantAsync(), CanAddOrUpdateEtudiant);
            UpdateEtudiantCommand = new RelayCommand(async param => await UpdateEtudiantAsync(), CanUpdateOrArchiveEtudiant);
            ArchiveEtudiantCommand = new RelayCommand(
                async param => await ArchiveEtudiantAsync(),
                param => SelectedEtudiant != null && CanUserArchive // Le bouton ne s'active que si un étudiant est sélectionné ET l'utilisateur a le droit
            );            ClearFormCommand = new RelayCommand(param => ClearInputFieldsAndSelection());

            _ = LoadEtudiantsAsync();
        }

        public async Task LoadEtudiantsAsync()
        {
            try
            {
                var etudiantsList = await _etudiantRepository.GetAllEtudiantsAsync();
                Etudiants = new ObservableCollection<Etudiant>(etudiantsList);
                StatusMessage = $"Chargement de {Etudiants.Count} étudiants réussi.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur chargement étudiants: {ex.Message}";
            }
        }

        private bool CanAddOrUpdateEtudiant(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(NomInput) && !string.IsNullOrWhiteSpace(PrenomInput);
            // Tu pourrais ajouter d'autres validations ici (ex: format date de naissance)
        }

        private async Task AddEtudiantAsync()
        {
            if (!CanAddOrUpdateEtudiant(null))
            {
                StatusMessage = "Nom et prénom sont requis.";
                return;
            }

            var nouvelEtudiant = new Etudiant
            {
                Nom = NomInput,
                Prenom = PrenomInput,
                DateNaissance = DateNaissanceInput, // Valider le format si nécessaire
                Adresse = AdresseInput,
                Telephone = TelephoneInput,
                Lycee = LyceeInput,
                Notes = NotesInput,
                EstArchive = false
                // DateInscriptionSysteme est gérée par le Repository ou le modèle
            };

            try
            {
                await _etudiantRepository.AddEtudiantAsync(nouvelEtudiant);
                await LoadEtudiantsAsync();
                ClearInputFieldsAndSelection();
                StatusMessage = $"Étudiant '{nouvelEtudiant.Prenom} {nouvelEtudiant.Nom}' ajouté.";
            }
            catch (InvalidOperationException ex) // Pour les erreurs d'unicité (ex: téléphone)
            {
                StatusMessage = ex.Message;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur ajout étudiant: {ex.Message}";
            }
        }
        
        private bool CanUpdateOrArchiveEtudiant(object? parameter)
        {
            return SelectedEtudiant != null && !string.IsNullOrWhiteSpace(NomInput) && !string.IsNullOrWhiteSpace(PrenomInput);
        }

        private bool CanArchiveEtudiant(object? parameter)
        {
            return SelectedEtudiant != null;
        }

        private async Task UpdateEtudiantAsync()
        {
            if (!CanUpdateOrArchiveEtudiant(null) || SelectedEtudiant == null) return;

            var etudiantAMettreAJour = new Etudiant
            {
                IDEtudiant = SelectedEtudiant.IDEtudiant,
                Nom = NomInput,
                Prenom = PrenomInput,
                DateNaissance = DateNaissanceInput,
                Adresse = AdresseInput,
                Telephone = TelephoneInput,
                Lycee = LyceeInput,
                Notes = NotesInput,
                DateInscriptionSysteme = SelectedEtudiant.DateInscriptionSysteme, // Conserver la date d'inscription originale
                EstArchive = SelectedEtudiant.EstArchive
            };

            try
            {
                await _etudiantRepository.UpdateEtudiantAsync(etudiantAMettreAJour);
                await LoadEtudiantsAsync();
                ClearInputFieldsAndSelection();
                StatusMessage = $"Étudiant '{etudiantAMettreAJour.Prenom} {etudiantAMettreAJour.Nom}' mis à jour.";
            }
            catch (InvalidOperationException ex)
            {
                StatusMessage = ex.Message;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur MàJ étudiant: {ex.Message}";
            }
        }

        private async Task ArchiveEtudiantAsync()
        {
            if (!CanArchiveEtudiant(null) || SelectedEtudiant == null) return;

            try
            {
                await _etudiantRepository.ArchiveEtudiantAsync(SelectedEtudiant.IDEtudiant);
                await LoadEtudiantsAsync();
                ClearInputFieldsAndSelection();
                StatusMessage = $"Étudiant '{SelectedEtudiant.Prenom} {SelectedEtudiant.Nom}' archivé.";
            }
            catch (InvalidOperationException ex) // Pour les erreurs du repo (ex: inscriptions actives)
            {
                StatusMessage = ex.Message;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur archivage étudiant: {ex.Message}";
            }
        }

        private void ClearInputFields()
        {
            NomInput = string.Empty;
            PrenomInput = string.Empty;
            DateNaissanceInput = null;
            AdresseInput = null;
            TelephoneInput = null;
            LyceeInput = null;
            NotesInput = null;
        }

        private void ClearInputFieldsAndSelection()
        {
            ClearInputFields();
            SelectedEtudiant = null;
            StatusMessage = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}