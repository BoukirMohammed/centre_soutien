using centre_soutien.Models;
using centre_soutien.DataAccess;
using centre_soutien.Commands; // Assure-toi que ce using pointe vers ta classe RelayCommand
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using centre_soutien.Views; // Ajoute ce using pour AddInscriptionWindow
using System.Windows;  
// using centre_soutien.Views; // Nécessaire si tu ouvres une fenêtre d'ajout depuis ici

namespace centre_soutien.ViewModels
{
    public class InscriptionViewModel : INotifyPropertyChanged
    {
        private readonly InscriptionRepository _inscriptionRepository;
        // On aura besoin des repositories pour charger les listes pour le formulaire d'ajout
        private readonly EtudiantRepository _etudiantRepository;
        private readonly GroupeRepository _groupeRepository;


        private ObservableCollection<Inscription> _inscriptions;
        public ObservableCollection<Inscription> Inscriptions
        {
            get => _inscriptions;
            set { _inscriptions = value; OnPropertyChanged(); }
        }

        private Inscription? _selectedInscription;
        public Inscription? SelectedInscription
        {
            get => _selectedInscription;
            set
            {
                _selectedInscription = value;
                OnPropertyChanged();
                (DesinscrireCommand as RelayCommand)?.RaiseCanExecuteChanged();
                // (EditInscriptionCommand as RelayCommand)?.RaiseCanExecuteChanged(); // Si tu implémentes la modification
            }
        }
        
        // --- Propriétés pour le formulaire d'ajout/modification (dans une fenêtre/dialogue séparé) ---
        // Ces propriétés seraient plutôt dans un ViewModel dédié pour la fenêtre d'ajout
        // mais pour simplifier au début, on peut les gérer ici et les passer à la fenêtre.
        private ObservableCollection<Etudiant> _allEtudiants = new ObservableCollection<Etudiant>();
        public ObservableCollection<Etudiant> AllEtudiants
        {
            get => _allEtudiants;
            set { _allEtudiants = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Groupe> _allGroupes = new ObservableCollection<Groupe>();
        public ObservableCollection<Groupe> AllGroupes
        {
            get => _allGroupes;
            set { _allGroupes = value; OnPropertyChanged(); }
        }

        // ... autres propriétés pour le formulaire d'ajout : SelectedEtudiantForNew, SelectedGroupeForNew, PrixConvenu, JourEcheance ...


        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        // Commandes
        public ICommand LoadInscriptionsCommand { get; }
        public ICommand OpenAddInscriptionDialogCommand { get; } // Ouvre la fenêtre d'ajout
        public ICommand DesinscrireCommand { get; }
        // public ICommand EditInscriptionCommand { get; } // Pour la modification

        public InscriptionViewModel()
        {
            _inscriptionRepository = new InscriptionRepository();
            _etudiantRepository = new EtudiantRepository(); // Initialiser
            _groupeRepository = new GroupeRepository();   // Initialiser

            Inscriptions = new ObservableCollection<Inscription>();

            LoadInscriptionsCommand = new RelayCommand(async param => await LoadAllDataAsync()); // Charger aussi listes pour dialogue
            OpenAddInscriptionDialogCommand = new RelayCommand(param => OpenAddInscriptionDialog());
            DesinscrireCommand = new RelayCommand(async param => await DesinscrireAsync(), CanDesinscrire);
            
            _ = LoadAllDataAsync(); // Charger les données initialement
        }

        public async Task LoadAllDataAsync() // Renommé pour refléter qu'on charge plus que les inscriptions
        {
            await LoadInscriptionsAsync();
            await LoadEtudiantsForDialogAsync();
            await LoadGroupesForDialogAsync();
        }

        public async Task LoadInscriptionsAsync() // Rendre public si appelé par MainViewModel
        {
            try
            {
                var inscriptionsList = await _inscriptionRepository.GetAllInscriptionsWithDetailsAsync();
                Inscriptions = new ObservableCollection<Inscription>(inscriptionsList);
                StatusMessage = $"Chargement de {Inscriptions.Count} inscriptions réussi.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur chargement inscriptions: {ex.Message}";
            }
        }
        
        private async Task LoadEtudiantsForDialogAsync()
        {
            try
            {
                var etudiantsList = await _etudiantRepository.GetAllEtudiantsAsync();
                AllEtudiants = new ObservableCollection<Etudiant>(etudiantsList);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur chargement étudiants pour dialogue: {ex.Message}";
            }
        }

        private async Task LoadGroupesForDialogAsync()
        {
            try
            {
                // Charger les groupes avec leurs détails (matière, prof) pour un affichage informatif dans la ComboBox
                var groupesList = await _groupeRepository.GetAllGroupesWithDetailsAsync();
                AllGroupes = new ObservableCollection<Groupe>(groupesList);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur chargement groupes pour dialogue: {ex.Message}";
            }
        }

        // Dans ViewModels/InscriptionViewModel.cs
     // Ajoute ce using pour Application.Current

// ... reste de la classe ...

        private async void OpenAddInscriptionDialog() // Rendre async si LoadInscriptionsAsync est appelé directement
        {
            // Créer le ViewModel pour la fenêtre d'ajout
            var addVM = new AddInscriptionViewModel(_etudiantRepository, _groupeRepository, _inscriptionRepository);

            var addInscriptionWindow = new AddInscriptionWindow
            {
                DataContext = addVM,
                Owner = Application.Current.MainWindow // Important pour le comportement modal et le centrage
            };

            bool? result = addInscriptionWindow.ShowDialog(); // Affiche la fenêtre en mode dialogue

            if (result == true) // Si DialogResult a été mis à true par la fenêtre (via InscriptionReussie)
            {
                await LoadInscriptionsAsync(); // Recharger la liste principale des inscriptions
                StatusMessage = addVM.StatusMessage; // Afficher le message de succès du dialogue
            }
            else if (!string.IsNullOrEmpty(addVM.StatusMessage) && addVM.StatusMessage != "Inscription réussie !")
            {
                // Afficher un message d'erreur du dialogue si l'opération a échoué ou a été annulée avec un message
                StatusMessage = addVM.StatusMessage;
            }
            else
            {
                StatusMessage = "Ajout d'inscription annulé.";
            }
        }
        private bool CanDesinscrire(object? parameter)
        {
            return SelectedInscription != null && SelectedInscription.EstActif;
        }

        private async Task DesinscrireAsync()
        {
            if (!CanDesinscrire(null) || SelectedInscription == null) return;

            // Ajouter une confirmation utilisateur
            // Exemple: if (MessageBox.Show(...) != MessageBoxResult.Yes) return;

            try
            {
                await _inscriptionRepository.DesinscrireAsync(SelectedInscription.IDInscription);
                await LoadInscriptionsAsync(); // Recharger la liste
                StatusMessage = $"Inscription désactivée pour {SelectedInscription.Etudiant?.NomComplet} au groupe {SelectedInscription.Groupe?.NomDescriptifGroupe}.";
                SelectedInscription = null; // Désélectionner
            }
            catch (InvalidOperationException ex) { StatusMessage = ex.Message; }
            catch (Exception ex) { StatusMessage = $"Erreur lors de la désinscription: {ex.Message}"; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}