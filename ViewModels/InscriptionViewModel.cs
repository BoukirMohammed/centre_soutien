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
using centre_soutien.Services.PDF;
using System.Linq;





// using centre_soutien.Views; // Nécessaire si tu ouvres une fenêtre d'ajout depuis ici

namespace centre_soutien.ViewModels
{
    public class InscriptionViewModel : INotifyPropertyChanged
    {
        private readonly InscriptionRepository _inscriptionRepository;

        // On aura besoin des repositories pour charger les listes pour le formulaire d'ajout
        private readonly EtudiantRepository _etudiantRepository;
        private readonly GroupeRepository _groupeRepository;
        private readonly IPdfExportService _pdfService;


        public ICommand ExportPdfCommand { get; }


        private ObservableCollection<Inscription> _inscriptions;

        public ObservableCollection<Inscription> Inscriptions
        {
            get => _inscriptions;
            set
            {
                _inscriptions = value;
                OnPropertyChanged();
            }
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
            set
            {
                _allEtudiants = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Groupe> _allGroupes = new ObservableCollection<Groupe>();

        public ObservableCollection<Groupe> AllGroupes
        {
            get => _allGroupes;
            set
            {
                _allGroupes = value;
                OnPropertyChanged();
            }
        }

        // ... autres propriétés pour le formulaire d'ajout : SelectedEtudiantForNew, SelectedGroupeForNew, PrixConvenu, JourEcheance ...


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

        // Commandes
        public ICommand LoadInscriptionsCommand { get; }
        public ICommand OpenAddInscriptionDialogCommand { get; } // Ouvre la fenêtre d'ajout
        public ICommand DesinscrireCommand { get; }
        // public ICommand EditInscriptionCommand { get; } // Pour la modification
        public InscriptionViewModel()
        {
            _inscriptionRepository = new InscriptionRepository();
            _etudiantRepository = new EtudiantRepository();
            _groupeRepository = new GroupeRepository();
            _pdfService = new PdfExportService(); // ← Une seule initialisation

            Inscriptions = new ObservableCollection<Inscription>();

            LoadInscriptionsCommand = new RelayCommand(async param => await LoadAllDataAsync());
            OpenAddInscriptionDialogCommand = new RelayCommand(param => OpenAddInscriptionDialog());
            DesinscrireCommand = new RelayCommand(async param => await DesinscrireAsync(), CanDesinscrire);
            ExportPdfCommand = new RelayCommand(async param => await ExportInscriptionsToPdfAsync()); // ← Une seule ligne

            _ = LoadAllDataAsync();
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
                StatusMessage =
                    $"Inscription désactivée pour {SelectedInscription.Etudiant?.NomComplet} au groupe {SelectedInscription.Groupe?.NomDescriptifGroupe}.";
                SelectedInscription = null; // Désélectionner
            }
            catch (InvalidOperationException ex)
            {
                StatusMessage = ex.Message;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors de la désinscription: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task ExportInscriptionsToPdfAsync()
        {
            try
            {
                StatusMessage = "Préparation de l'export PDF des inscriptions...";
                System.Diagnostics.Debug.WriteLine("=== DÉBUT EXPORT PDF INSCRIPTIONS ===");

                if (Inscriptions == null || !Inscriptions.Any())
                {
                    StatusMessage = "Aucune inscription à exporter.";
                    System.Diagnostics.Debug.WriteLine("ERREUR: Aucune inscription trouvée");
                    MessageBox.Show("❌ Aucune inscription trouvée pour l'export.", "Liste vide",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Nombre d'inscriptions à exporter: {Inscriptions.Count}");

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Fichiers PDF (*.pdf)|*.pdf",
                    DefaultExt = "pdf",
                    FileName = $"Liste_Inscriptions_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
                };

                System.Diagnostics.Debug.WriteLine("Ouverture du dialogue de sauvegarde...");

                var dialogResult = saveDialog.ShowDialog();
                System.Diagnostics.Debug.WriteLine($"Résultat du dialogue: {dialogResult}");

                if (dialogResult == true)
                {
                    System.Diagnostics.Debug.WriteLine($"Fichier sélectionné: {saveDialog.FileName}");
                    StatusMessage = "Export PDF en cours...";

                    // Vérifier que le service PDF est initialisé
                    if (_pdfService == null)
                    {
                        System.Diagnostics.Debug.WriteLine("ERREUR: _pdfService est null!");
                        StatusMessage = "Erreur: Service PDF non initialisé";
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine("Appel du service PDF pour inscriptions...");

                    bool success = await _pdfService.ExportInscriptionsAsync(
                        Inscriptions.ToList(), saveDialog.FileName);

                    System.Diagnostics.Debug.WriteLine($"Résultat de l'export: {success}");

                    if (success)
                    {
                        StatusMessage = "Export PDF réussi !";
                        System.Diagnostics.Debug.WriteLine("=== EXPORT PDF INSCRIPTIONS RÉUSSI ===");

                        // Vérifier que le fichier existe vraiment
                        if (System.IO.File.Exists(saveDialog.FileName))
                        {
                            var fileInfo = new System.IO.FileInfo(saveDialog.FileName);
                            System.Diagnostics.Debug.WriteLine(
                                $"Fichier créé: {fileInfo.FullName}, Taille: {fileInfo.Length} bytes");

                            MessageBox.Show(
                                $"✅ Export terminé avec succès !\n\nFichier sauvegardé : {saveDialog.FileName}\nTaille: {fileInfo.Length} bytes",
                                "Export réussi", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("ERREUR: Le fichier n'existe pas après l'export!");
                            StatusMessage = "Erreur: Fichier non créé";
                            MessageBox.Show("❌ Le fichier PDF n'a pas été créé correctement.",
                                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        StatusMessage = "Erreur lors de l'export PDF";
                        System.Diagnostics.Debug.WriteLine("ERREUR: L'export a échoué");
                        MessageBox.Show(
                            "❌ Une erreur est survenue lors de l'export PDF.\nVérifiez les logs pour plus de détails.",
                            "Erreur d'export", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    StatusMessage = "Export annulé par l'utilisateur.";
                    System.Diagnostics.Debug.WriteLine("Export annulé par l'utilisateur");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur : {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"EXCEPTION: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"STACK TRACE: {ex.StackTrace}");
                MessageBox.Show($"❌ Erreur inattendue lors de l'export :\n\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}