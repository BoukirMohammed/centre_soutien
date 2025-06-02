using centre_soutien.Models;
using centre_soutien.DataAccess;
using centre_soutien.Commands; // Assure-toi que ce using pointe vers ta classe RelayCommand
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using centre_soutien.Views; // Pour GestionMatieresProfesseurWindow
using System.Windows; 
namespace centre_soutien.ViewModels
{
    public class ProfesseurViewModel : INotifyPropertyChanged
    {
        private readonly ProfesseurRepository _professeurRepository;
        private readonly MatiereRepository _matiereRepository; // Nécessaire pour le dialogue
        private readonly ProfesseurMatiereRepository _profMatiereRepository; // Nécessaire
        private ObservableCollection<Professeur> _professeurs;
        public ObservableCollection<Professeur> Professeurs
        {
            get => _professeurs;
            set { _professeurs = value; OnPropertyChanged(); }
        }

        private Professeur? _selectedProfesseur; // Nullable
        public Professeur? SelectedProfesseur
        {
            get => _selectedProfesseur;
            set
            {
                _selectedProfesseur = value;
                OnPropertyChanged();
                if (_selectedProfesseur != null)
                {
                    NomInput = _selectedProfesseur.Nom;
                    PrenomInput = _selectedProfesseur.Prenom;
                    TelephoneInput = _selectedProfesseur.Telephone;
                    NotesInput = _selectedProfesseur.Notes;
                }
                else
                {
                    ClearInputFields();
                }
                (AddProfesseurCommand as RelayCommand)?.RaiseCanExecuteChanged(); // Utilise l'extension si tu l'as
                (UpdateProfesseurCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ArchiveProfesseurCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Champs pour le formulaire
        private string _nomInput = string.Empty;
        public string NomInput
        {
            get => _nomInput;
            set { _nomInput = value; OnPropertyChanged(); (AddProfesseurCommand as RelayCommand)?.RaiseCanExecuteChanged(); (UpdateProfesseurCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private string _prenomInput = string.Empty;
        public string PrenomInput
        {
            get => _prenomInput;
            set { _prenomInput = value; OnPropertyChanged(); (AddProfesseurCommand as RelayCommand)?.RaiseCanExecuteChanged(); (UpdateProfesseurCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private string? _telephoneInput; // Nullable
        public string? TelephoneInput
        {
            get => _telephoneInput;
            set { _telephoneInput = value; OnPropertyChanged(); }
        }

        private string? _notesInput; // Nullable
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
        public ICommand LoadProfesseursCommand { get; }
        public ICommand AddProfesseurCommand { get; }
        public ICommand UpdateProfesseurCommand { get; }
        public ICommand ArchiveProfesseurCommand { get; }
        public ICommand ClearFormCommand { get; }
        public ICommand OpenGestionMatieresProfesseurCommand { get; } // <<< NOUVELLE COMMANDE

        public ProfesseurViewModel()
        {
            _professeurRepository = new ProfesseurRepository();
            _matiereRepository = new MatiereRepository(); // <<< INITIALISER
            _profMatiereRepository = new ProfesseurMatiereRepository(); // <<< INITIALISER

            Professeurs = new ObservableCollection<Professeur>();

            LoadProfesseursCommand = new RelayCommand(async param => await LoadProfesseursAsync());
            AddProfesseurCommand = new RelayCommand(async param => await AddProfesseurAsync(), CanAddOrUpdateProfesseur);
            UpdateProfesseurCommand = new RelayCommand(async param => await UpdateProfesseurAsync(), CanUpdateOrArchiveProfesseur);
            ArchiveProfesseurCommand = new RelayCommand(async param => await ArchiveProfesseurAsync(), CanArchiveProfesseur);
            ClearFormCommand = new RelayCommand(param => ClearInputFieldsAndSelection());
            OpenGestionMatieresProfesseurCommand = new RelayCommand(param => OpenGestionMatieresProfesseurDialog(), CanOpenGestionMatieresProfesseurDialog); // <<< INITIALISER NOUVELLE COMMANDE


            _ = LoadProfesseursAsync();
        }

        public async Task LoadProfesseursAsync()
        {
            try
            {
                var professeursList = await _professeurRepository.GetAllProfesseursAsync();
                Professeurs = new ObservableCollection<Professeur>(professeursList);
                StatusMessage = $"Chargement de {Professeurs.Count} professeurs réussi.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur chargement professeurs: {ex.Message}";
            }
        }

        private bool CanAddOrUpdateProfesseur(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(NomInput) && !string.IsNullOrWhiteSpace(PrenomInput);
        }

        private async Task AddProfesseurAsync()
        {
            if (!CanAddOrUpdateProfesseur(null))
            {
                StatusMessage = "Nom et prénom sont requis.";
                return;
            }

            var nouveauProfesseur = new Professeur
            {
                Nom = NomInput,
                Prenom = PrenomInput,
                Telephone = TelephoneInput,
                Notes = NotesInput,
                EstArchive = false
            };

            try
            {
                await _professeurRepository.AddProfesseurAsync(nouveauProfesseur);
                await LoadProfesseursAsync();
                ClearInputFieldsAndSelection();
                StatusMessage = $"Professeur '{nouveauProfesseur.Prenom} {nouveauProfesseur.Nom}' ajouté.";
            }
            catch (Exception ex) // Devrait être plus spécifique pour les erreurs d'unicité si implémenté dans le repo
            {
                StatusMessage = $"Erreur ajout professeur: {ex.Message}";
            }
        }
        
        private bool CanUpdateOrArchiveProfesseur(object? parameter)
        {
            return SelectedProfesseur != null && !string.IsNullOrWhiteSpace(NomInput) && !string.IsNullOrWhiteSpace(PrenomInput);
        }
        private bool CanArchiveProfesseur(object? parameter)
        {
            return SelectedProfesseur != null;
        }

        private async Task UpdateProfesseurAsync()
        {
            if (!CanUpdateOrArchiveProfesseur(null) || SelectedProfesseur == null) return;

            var professeurAMettreAJour = new Professeur
            {
                IDProfesseur = SelectedProfesseur.IDProfesseur,
                Nom = NomInput,
                Prenom = PrenomInput,
                Telephone = TelephoneInput,
                Notes = NotesInput,
                EstArchive = SelectedProfesseur.EstArchive // Conserver l'état d'archivage
            };

            try
            {
                await _professeurRepository.UpdateProfesseurAsync(professeurAMettreAJour);
                await LoadProfesseursAsync();
                ClearInputFieldsAndSelection();
                StatusMessage = $"Professeur '{professeurAMettreAJour.Prenom} {professeurAMettreAJour.Nom}' mis à jour.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur MàJ professeur: {ex.Message}";
            }
        }

        private async Task ArchiveProfesseurAsync()
        {
            if (!CanArchiveProfesseur(null) || SelectedProfesseur == null) return;

            // Ajouter une confirmation utilisateur ici serait bien
            try
            {
                await _professeurRepository.ArchiveProfesseurAsync(SelectedProfesseur.IDProfesseur);
                await LoadProfesseursAsync();
                ClearInputFieldsAndSelection();
                StatusMessage = $"Professeur '{SelectedProfesseur.Prenom} {SelectedProfesseur.Nom}' archivé.";
            }
            catch (InvalidOperationException ex) // Pour les erreurs du repo (ex: prof assigné)
            {
                StatusMessage = ex.Message;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur archivage professeur: {ex.Message}";
            }
        }

        private void ClearInputFields()
        {
            NomInput = string.Empty;
            PrenomInput = string.Empty;
            TelephoneInput = null; // ou string.Empty si tu préfères
            NotesInput = null;     // ou string.Empty
        }

        private void ClearInputFieldsAndSelection()
        {
            ClearInputFields();
            SelectedProfesseur = null;
            StatusMessage = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged; // Nullable
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) // Nullable
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private bool CanOpenGestionMatieresProfesseurDialog(object? parameter)
        {
            return SelectedProfesseur != null; // On ne peut ouvrir que si un prof est sélectionné
        }

        private void OpenGestionMatieresProfesseurDialog()
        {
            if (SelectedProfesseur == null) return;

            // Créer le ViewModel pour la fenêtre de dialogue
            var gestionMatieresVM = new GestionMatieresProfesseurViewModel(
                SelectedProfesseur.IDProfesseur,
                _professeurRepository, // Peut ne pas être nécessaire si le dialogue ne modifie pas le prof lui-même
                _matiereRepository,
                _profMatiereRepository
            );

            var gestionMatieresWindow = new GestionMatieresProfesseurWindow
            {
                DataContext = gestionMatieresVM,
                Owner = Application.Current.MainWindow
            };

            gestionMatieresWindow.ShowDialog(); // Affiche en mode dialogue

            // Optionnel: recharger des données si nécessaire après la fermeture du dialogue
            // Par exemple, si l'affichage du professeur principal doit refléter un changement
            // (mais ici, les changements sont dans une table séparée, donc pas forcément direct)
            // _ = LoadProfesseursAsync(); // Peut-être pas nécessaire ici
            StatusMessage = gestionMatieresVM.StatusMessage; // Récupérer le dernier message du dialogue
        }
    }
}