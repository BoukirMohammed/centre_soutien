using centre_soutien.Models;
using centre_soutien.DataAccess;
using centre_soutien.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using centre_soutien.Services; // Pour CurrentUserSession
using centre_soutien.Views;
using System.Windows;

namespace centre_soutien.ViewModels
{
    public class GroupeViewModel : INotifyPropertyChanged
    {
        private readonly GroupeRepository _groupeRepository;
        private readonly MatiereRepository _matiereRepository;
        private readonly ProfesseurRepository _professeurRepository;
        private readonly InscriptionRepository _inscriptionRepository;

        // --- Pour la sélection du Professeur dont on affiche les groupes ---
        private ObservableCollection<Professeur> _allProfesseursActifs;
        public ObservableCollection<Professeur> AllProfesseursActifs
        {
            get => _allProfesseursActifs;
            set { _allProfesseursActifs = value; OnPropertyChanged(); }
        }

        private Professeur? _selectedProfesseurForFilter;
        public Professeur? SelectedProfesseurForFilter
        {
            get => _selectedProfesseurForFilter;
            set
            {
                if (_selectedProfesseurForFilter != value) // Agir seulement si la sélection change
                {
                    _selectedProfesseurForFilter = value;
                    OnPropertyChanged();
                    _ = LoadGroupesForSelectedProfesseurAsync(); // Charger les groupes du nouveau prof
                    ClearFormAndSelectedGroupe(); // Vider le formulaire et la sélection de groupe
                }
            }
        }

        // --- Collection principale pour afficher les groupes du professeur sélectionné ---
        private ObservableCollection<Groupe> _groupesAAfficher;
        public ObservableCollection<Groupe> GroupesAAfficher // Anciennement GroupesDuProfesseurSelectionne
        {
            get => _groupesAAfficher;
            set { _groupesAAfficher = value; OnPropertyChanged(); }
        }

        // --- Pour le groupe sélectionné dans la liste GroupesAAfficher ---
        private Groupe? _selectedGroupe;
        public Groupe? SelectedGroupe
        {
            get => _selectedGroupe;
            set
            {
                _selectedGroupe = value;
                OnPropertyChanged();
                PopulateFormForEdit(); // Mettre à jour le formulaire avec les données du groupe sélectionné
                // Mettre à jour l'état CanExecute des commandes liées à la sélection d'un groupe
                (SaveGroupeCommand as RelayCommand)?.RaiseCanExecuteChanged(); // Au lieu de UpdateGroupeCommand
                (ArchiveGroupeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ShowEtudiantsInscritsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
        
        // --- Collections pour les ComboBox du formulaire d'ajout/modification de groupe ---
        private ObservableCollection<Matiere> _allMatieres;
        public ObservableCollection<Matiere> AllMatieres 
        { get => _allMatieres; set { _allMatieres = value; OnPropertyChanged(); } }
        // Note: AllProfesseursActifs sera aussi utilisé pour la ComboBox des profs dans le formulaire

        // --- Propriétés pour les champs de saisie du formulaire de groupe ---
        private string _nomDescriptifInput = string.Empty;
        public string NomDescriptifInput { get => _nomDescriptifInput; set { _nomDescriptifInput = value; OnPropertyChanged(); ValidateForm(); } }
        
        private Matiere? _selectedMatiereForForm;
        public Matiere? SelectedMatiereForForm { get => _selectedMatiereForForm; set { _selectedMatiereForForm = value; OnPropertyChanged(); ValidateForm(); } }

        private Professeur? _selectedProfesseurForForm; // Professeur pour le groupe en cours de création/modification
        public Professeur? SelectedProfesseurForForm 
        { get => _selectedProfesseurForForm; set { _selectedProfesseurForForm = value; OnPropertyChanged(); ValidateForm(); } }
        
        private string? _niveauInput;
        public string? NiveauInput { get => _niveauInput; set { _niveauInput = value; OnPropertyChanged(); } }

        private string? _notesInput;
        public string? NotesInput { get => _notesInput; set { _notesInput = value; OnPropertyChanged(); } }

        private string _statusMessage = string.Empty;
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }
        
        public bool CanUserArchive => CurrentUserSession.IsAdmin; // Pour contrôler l'archivage


        // Commandes
        public ICommand LoadDataCommand { get; }
        public ICommand PrepareAddGroupeCommand { get; } // Pour mettre le formulaire en mode ajout
        public ICommand SaveGroupeCommand { get; }      // Gère Ajout ET Modification
        public ICommand ArchiveGroupeCommand { get; }
        public ICommand CancelEditCommand { get; }      // Annuler l'ajout/modification en cours
        public ICommand ShowEtudiantsInscritsCommand { get; }

        private bool _isEditing = false;
        public bool IsEditing { get => _isEditing; set { _isEditing = value; OnPropertyChanged(); ValidateForm(); } }


        public GroupeViewModel()
        {
            _groupeRepository = new GroupeRepository();
            _matiereRepository = new MatiereRepository();
            _professeurRepository = new ProfesseurRepository();
            _inscriptionRepository = new InscriptionRepository();

            AllProfesseursActifs = new ObservableCollection<Professeur>();
            GroupesAAfficher = new ObservableCollection<Groupe>();
            AllMatieres = new ObservableCollection<Matiere>();

            LoadDataCommand = new RelayCommand(async param => await LoadAllInitialDataAsync());
            PrepareAddGroupeCommand = new RelayCommand(param => PrepareForAdd(), param => SelectedProfesseurForFilter != null); // Actif seulement si un prof est sélectionné
            SaveGroupeCommand = new RelayCommand(async param => await SaveGroupeAsync(), CanSaveGroupe);
            ArchiveGroupeCommand = new RelayCommand(async param => await ArchiveGroupeAsync(), CanArchiveGroupe);
            CancelEditCommand = new RelayCommand(param => CancelCurrentEdit());
            ShowEtudiantsInscritsCommand = new RelayCommand(param => OpenEtudiantsInscritsDialog(), CanShowEtudiantsInscrits);
            
            _ = LoadAllInitialDataAsync();
        }

        private void PopulateFormForEdit()
        {
            if (_selectedGroupe != null)
            {
                IsEditing = true; // Passer en mode édition
                NomDescriptifInput = _selectedGroupe.NomDescriptifGroupe;
                SelectedMatiereForForm = AllMatieres.FirstOrDefault(m => m.IDMatiere == _selectedGroupe.IDMatiere);
                SelectedProfesseurForForm = AllProfesseursActifs.FirstOrDefault(p => p.IDProfesseur == _selectedGroupe.IDProfesseur);
                NiveauInput = _selectedGroupe.Niveau;
                NotesInput = _selectedGroupe.Notes;
            }
        }

        public async Task LoadAllInitialDataAsync()
        {
            await LoadProfesseursAsync();
            await LoadMatieresForComboBoxAsync();
            // Ne pas charger les groupes ici, attendre la sélection d'un professeur
            if (AllProfesseursActifs.Any())
            {
                SelectedProfesseurForFilter = AllProfesseursActifs.First(); // Sélectionner le premier prof par défaut
            }
            else
            {
                GroupesAAfficher.Clear();
            }
        }

        private async Task LoadProfesseursAsync()
        {
            try
            {
                var profs = await _professeurRepository.GetAllProfesseursAsync(); // Récupère les actifs par défaut
                AllProfesseursActifs = new ObservableCollection<Professeur>(profs);
            }
            catch (Exception ex) { StatusMessage = $"Erreur chargement professeurs: {ex.Message}"; }
        }
        
        private async Task LoadMatieresForComboBoxAsync()
        {
            try
            {
                var matieresList = await _matiereRepository.GetAllMatieresAsync(); 
                AllMatieres = new ObservableCollection<Matiere>(matieresList);
            }
            catch (Exception ex) { StatusMessage = $"Erreur chargement matières: {ex.Message}"; }
        }

        private async Task LoadGroupesForSelectedProfesseurAsync()
        {
            GroupesAAfficher.Clear(); // Vider la liste précédente
            SelectedGroupe = null; // Désélectionner le groupe
            if (SelectedProfesseurForFilter == null) return;

            try
            {
                var tousLesGroupes = await _groupeRepository.GetAllGroupesWithDetailsAsync();
                var groupesFiltres = tousLesGroupes
                    .Where(g => g.IDProfesseur == SelectedProfesseurForFilter.IDProfesseur && !g.EstArchive)
                    .OrderBy(g => g.NomDescriptifGroupe)
                    .ToList();
                GroupesAAfficher = new ObservableCollection<Groupe>(groupesFiltres);
                StatusMessage = $"{GroupesAAfficher.Count} groupe(s) trouvé(s) pour {SelectedProfesseurForFilter.NomComplet}.";
            }
            catch (Exception ex) { StatusMessage = $"Erreur chargement groupes pour {SelectedProfesseurForFilter.NomComplet}: {ex.Message}"; }
        }

        private void PrepareForAdd()
        {
            if (SelectedProfesseurForFilter == null)
            {
                StatusMessage = "Veuillez d'abord sélectionner un professeur.";
                return;
            }
            SelectedGroupe = null; // Désélectionner pour être sûr qu'on n'est pas en train d'éditer
            IsEditing = true;
            ClearInputFields();
            SelectedProfesseurForForm = AllProfesseursActifs.FirstOrDefault(p => p.IDProfesseur == SelectedProfesseurForFilter.IDProfesseur); // Pré-remplir le prof
            StatusMessage = $"Ajout d'un nouveau groupe pour {SelectedProfesseurForFilter.NomComplet}...";
        }
        
        private bool CanSaveGroupe(object? parameter)
        {
            return IsEditing && // On ne peut sauvegarder que si on est en mode édition/ajout
                   !string.IsNullOrWhiteSpace(NomDescriptifInput) &&
                   SelectedMatiereForForm != null &&
                   SelectedProfesseurForForm != null;
        }

        private void ValidateForm() // Appelé par les setters des inputs du formulaire
        {
             (SaveGroupeCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private async Task SaveGroupeAsync()
        {
            if (!CanSaveGroupe(null)) return;

            var groupeToSave = SelectedGroupe ?? new Groupe(); // Si SelectedGroupe est null, c'est un ajout

            groupeToSave.NomDescriptifGroupe = NomDescriptifInput;
            groupeToSave.IDMatiere = SelectedMatiereForForm!.IDMatiere;
            groupeToSave.IDProfesseur = SelectedProfesseurForForm!.IDProfesseur;
            groupeToSave.Niveau = NiveauInput;
            groupeToSave.Notes = NotesInput;
            groupeToSave.EstArchive = SelectedGroupe?.EstArchive ?? false; // Conserver l'état si update, false si ajout

            try
            {
                if (SelectedGroupe == null) // Ajout
                {
                    await _groupeRepository.AddGroupeAsync(groupeToSave);
                    StatusMessage = $"Groupe '{groupeToSave.NomDescriptifGroupe}' ajouté.";
                }
                else // Modification
                {
                    await _groupeRepository.UpdateGroupeAsync(groupeToSave);
                    StatusMessage = $"Groupe '{groupeToSave.NomDescriptifGroupe}' mis à jour.";
                }
                await LoadGroupesForSelectedProfesseurAsync();
                CancelCurrentEdit(); // Sortir du mode édition et vider le formulaire
            }
            catch (InvalidOperationException ex) { StatusMessage = ex.Message; }
            catch (Exception ex) { StatusMessage = $"Erreur sauvegarde groupe: {ex.Message}"; }
        }
        
        private bool CanArchiveGroupe(object? parameter) => SelectedGroupe != null && CanUserArchive; // Ajout de CanUserArchive
        private async Task ArchiveGroupeAsync()
        {
            if (!CanArchiveGroupe(null) || SelectedGroupe == null) return;
            try
            {
                await _groupeRepository.ArchiveGroupeAsync(SelectedGroupe.IDGroupe);
                await LoadGroupesForSelectedProfesseurAsync();
                CancelCurrentEdit();
                StatusMessage = $"Groupe '{SelectedGroupe.NomDescriptifGroupe}' archivé.";
            }
            catch (InvalidOperationException ex) { StatusMessage = ex.Message; }
            catch (Exception ex) { StatusMessage = $"Erreur archivage: {ex.Message}"; }
        }
        
        private void CancelCurrentEdit()
        {
            IsEditing = false;
            SelectedGroupe = null; // Cela va appeler ClearInputFields via le setter de SelectedGroupe
            StatusMessage = "Opération annulée ou terminée.";
        }

        private void ClearInputFields()
        {
            NomDescriptifInput = string.Empty;
            SelectedMatiereForForm = null;
            if (SelectedProfesseurForFilter != null)
            {
                SelectedProfesseurForForm = AllProfesseursActifs.FirstOrDefault(p => p.IDProfesseur == SelectedProfesseurForFilter.IDProfesseur);
            }
            else
            {
                SelectedProfesseurForForm = null;
            }
            NiveauInput = string.Empty; // string? peut être string.Empty
            NotesInput = string.Empty;  // string? peut être string.Empty
        }

        private void ClearFormAndSelectedGroupe() // Appelé par le bouton et après sélection d'un nouveau prof
        {
            IsEditing = false; // Sortir du mode édition
            ClearInputFields();
            SelectedGroupe = null;
            StatusMessage = string.Empty;
        }
        
        private bool CanShowEtudiantsInscrits(object? parameter) => SelectedGroupe != null;
        private void OpenEtudiantsInscritsDialog()
        {
            if (SelectedGroupe == null) return;
            var etudiantsInscritsVM = new EtudiantsInscritsViewModel(SelectedGroupe.IDGroupe, _inscriptionRepository, _groupeRepository);
            var etudiantsInscritsWindow = new EtudiantsInscritsWindow
            {
                DataContext = etudiantsInscritsVM,
                Owner = Application.Current.MainWindow
            };
            etudiantsInscritsWindow.ShowDialog();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}