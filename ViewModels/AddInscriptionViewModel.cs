using centre_soutien.Models;
using centre_soutien.DataAccess;
using centre_soutien.Commands; // Pour RelayCommand
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq; // Pour FirstOrDefault

namespace centre_soutien.ViewModels
{
    public class AddInscriptionViewModel : INotifyPropertyChanged
    {
        private readonly EtudiantRepository _etudiantRepository;
        private readonly GroupeRepository _groupeRepository;
        private readonly InscriptionRepository _inscriptionRepository;

        // Propriété pour indiquer si l'inscription a réussi (pour fermer la fenêtre)
        private bool _inscriptionReussie = false;
        public bool InscriptionReussie
        {
            get => _inscriptionReussie;
            private set { _inscriptionReussie = value; OnPropertyChanged(); }
        }

        // Collections pour les ComboBox
        private ObservableCollection<Etudiant> _allEtudiants;
        public ObservableCollection<Etudiant> AllEtudiants
        {
            get => _allEtudiants;
            set { _allEtudiants = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Groupe> _allGroupes;
        public ObservableCollection<Groupe> AllGroupes
        {
            get => _allGroupes;
            set { _allGroupes = value; OnPropertyChanged(); }
        }

        // --- Propriétés pour les sélections et saisies du formulaire ---
        private Etudiant? _selectedEtudiant;
        public Etudiant? SelectedEtudiant
        {
            get => _selectedEtudiant;
            set { _selectedEtudiant = value; OnPropertyChanged(); (InscrireCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private Groupe? _selectedGroupe;
        public Groupe? SelectedGroupe
        {
            get => _selectedGroupe;
            set
            {
                _selectedGroupe = value;
                OnPropertyChanged();
                // Pré-remplir le prix avec le prix standard de la matière du groupe si un groupe est sélectionné
                if (_selectedGroupe?.Matiere != null)
                {
                    PrixConvenuInput = _selectedGroupe.Matiere.PrixStandardMensuel;
                }
                (InscrireCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private double? _prixConvenuInput;
        public double? PrixConvenuInput
        {
            get => _prixConvenuInput;
            set { _prixConvenuInput = value; OnPropertyChanged(); (InscrireCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        private int? _jourEcheanceInput; // Doit être entre 1 et 31
        public int? JourEcheanceInput
        {
            get => _jourEcheanceInput;
            set { _jourEcheanceInput = value; OnPropertyChanged(); (InscrireCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }
        
        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        // Commandes
        public ICommand InscrireCommand { get; }
        // La commande Annuler sera gérée par la fenêtre elle-même (bouton avec IsCancel=true)

        public AddInscriptionViewModel(EtudiantRepository etudiantRepo, GroupeRepository groupeRepo, InscriptionRepository inscriptionRepo)
        {
            _etudiantRepository = etudiantRepo;
            _groupeRepository = groupeRepo;
            _inscriptionRepository = inscriptionRepo;

            AllEtudiants = new ObservableCollection<Etudiant>();
            AllGroupes = new ObservableCollection<Groupe>();

            InscrireCommand = new RelayCommand(async param => await InscrireAsync(), CanInscrire);

            // Suggérer le jour actuel comme jour d'échéance par défaut
            JourEcheanceInput = DateTime.Now.Day;

            _ = LoadInitialDataAsync(); // Charger les listes pour les ComboBox
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                var etudiantsList = await _etudiantRepository.GetAllEtudiantsAsync(); // Que les actifs
                AllEtudiants = new ObservableCollection<Etudiant>(etudiantsList);

                var groupesList = await _groupeRepository.GetAllGroupesWithDetailsAsync(); // Que les actifs, avec détails matière/prof
                AllGroupes = new ObservableCollection<Groupe>(groupesList);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur chargement données pour formulaire: {ex.Message}";
            }
        }

        private bool CanInscrire(object? parameter)
        {
            return SelectedEtudiant != null &&
                   SelectedGroupe != null &&
                   PrixConvenuInput.HasValue && PrixConvenuInput.Value >= 0 &&
                   JourEcheanceInput.HasValue && JourEcheanceInput.Value >= 1 && JourEcheanceInput.Value <= 31;
        }

        private async Task InscrireAsync()
        {
            if (!CanInscrire(null))
            {
                StatusMessage = "Veuillez remplir tous les champs correctement.";
                return;
            }

            var nouvelleInscription = new Inscription
            {
                IDEtudiant = SelectedEtudiant!.IDEtudiant, // ! car vérifié dans CanInscrire
                IDGroupe = SelectedGroupe!.IDGroupe,     // ! car vérifié
                PrixConvenuMensuel = PrixConvenuInput!.Value, // ! car vérifié
                JourEcheanceMensuelle = JourEcheanceInput!.Value, // ! car vérifié
                // DateInscription et EstActif seront gérés par le Repository ou initialisés par défaut dans le modèle Inscription
            };

            try
            {
                await _inscriptionRepository.AddInscriptionAsync(nouvelleInscription);
                StatusMessage = "Inscription réussie !";
                InscriptionReussie = true; // Pour signaler à la vue de se fermer
            }
            catch (InvalidOperationException ex) // Ex: Étudiant déjà inscrit
            {
                StatusMessage = ex.Message;
                InscriptionReussie = false;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erreur lors de l'inscription: {ex.Message}";
                InscriptionReussie = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}