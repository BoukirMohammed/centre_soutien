using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading; // Pour le timer
using System;
using centre_soutien.Commands;
using centre_soutien.Services;

namespace centre_soutien.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DispatcherTimer _timer;

        private object? _currentViewViewModel;
        public object? CurrentViewViewModel
        {
            get => _currentViewViewModel;
            set
            {
                _currentViewViewModel = value;
                OnPropertyChanged();
                UpdateCurrentViewTitle();
            }
        }

        private string _currentViewTitle = "Tableau de bord";
        public string CurrentViewTitle
        {
            get => _currentViewTitle;
            set
            {
                _currentViewTitle = value;
                OnPropertyChanged();
            }
        }

        private DateTime _currentDateTime = DateTime.Now;
        public DateTime CurrentDateTime
        {
            get => _currentDateTime;
            set
            {
                _currentDateTime = value;
                OnPropertyChanged();
            }
        }

        // Instances des ViewModels pour chaque section
        public SalleViewModel SalleVM { get; private set; }
        public MatiereViewModel MatiereVM { get; private set; }
        public ProfesseurViewModel ProfesseurVM { get; private set; }
        public EtudiantViewModel EtudiantVM { get; private set; }
        public GroupeViewModel GroupeVM { get; private set; }
        public InscriptionViewModel InscriptionVM { get; private set; }
        public PlanningViewModel PlanningVM { get; private set; }
        public GestionUtilisateursViewModel? GestionUtilisateursVM { get; private set; }
        public PaiementViewModel? PaimentVM { get; private set; }


        // Commandes pour la navigation
        public ICommand ShowSallesViewCommand { get; }
        public ICommand ShowMatieresViewCommand { get; }
        public ICommand ShowProfesseursViewCommand { get; }
        public ICommand ShowEtudiantsViewCommand { get; }
        public ICommand ShowGroupesViewCommand { get; }
        public ICommand ShowInscriptionsViewCommand { get; }
        public ICommand ShowPlanningViewCommand { get; }
        public ICommand? ShowGestionUtilisateursCommand { get; }
        public ICommand? ShowGestionPaiementCommand { get; }


        // Propriétés pour l'interface utilisateur
        public bool IsAdmin { get; private set; }
        public bool IsSecretaire { get; private set; }
        public string? NomUtilisateurConnecte { get; private set; }
        public string CurrentUserRole { get; private set; } = "";

        public MainViewModel()
        {
            // Récupérer les infos de l'utilisateur connecté
            if (CurrentUserSession.IsUserLoggedIn)
            {
                IsAdmin = CurrentUserSession.IsAdmin;
                IsSecretaire = CurrentUserSession.IsSecretaire;
                NomUtilisateurConnecte = CurrentUserSession.CurrentUser?.NomComplet ?? CurrentUserSession.CurrentUser?.Login;
                CurrentUserRole = CurrentUserSession.CurrentUser?.Role ?? "Utilisateur";
            }
            else
            {
                IsAdmin = false; 
                IsSecretaire = false;
                NomUtilisateurConnecte = "Utilisateur non connecté";
                CurrentUserRole = "Aucun";
            }

            // Initialiser le timer pour l'heure
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) => CurrentDateTime = DateTime.Now;
            _timer.Start();

            // Initialiser les ViewModels enfants
            SalleVM = new SalleViewModel();
            MatiereVM = new MatiereViewModel();
            ProfesseurVM = new ProfesseurViewModel();
            EtudiantVM = new EtudiantViewModel();
            GroupeVM = new GroupeViewModel();
            InscriptionVM = new InscriptionViewModel();
            PlanningVM = new PlanningViewModel();
            PaimentVM = new PaiementViewModel();
            if (IsAdmin)
            {
                GestionUtilisateursVM = new GestionUtilisateursViewModel();
                ShowGestionUtilisateursCommand = new RelayCommand(
                    async param => await NavigateTo(GestionUtilisateursVM), 
                    param => CanNavigateToGestionUtilisateurs(param));
            }

            // Définir les commandes de navigation
            ShowSallesViewCommand = new RelayCommand(async param => await NavigateTo(SalleVM));
            ShowMatieresViewCommand = new RelayCommand(async param => await NavigateTo(MatiereVM));
            ShowProfesseursViewCommand = new RelayCommand(async param => await NavigateTo(ProfesseurVM));
            ShowEtudiantsViewCommand = new RelayCommand(async param => await NavigateTo(EtudiantVM));
            ShowGroupesViewCommand = new RelayCommand(async param => await NavigateTo(GroupeVM));
            ShowInscriptionsViewCommand = new RelayCommand(async param => await NavigateTo(InscriptionVM));
            ShowPlanningViewCommand = new RelayCommand(async param => await NavigateTo(PlanningVM));
            ShowGestionPaiementCommand = new RelayCommand(async param => await NavigateTo(PaimentVM));

            // Vue par défaut
            if (CurrentUserSession.IsUserLoggedIn)
            {
                _ = NavigateTo(EtudiantVM); // Commencer par les étudiants (plus pertinent pour la secrétaire)
            }
        }
        
        private bool CanNavigateToGestionUtilisateurs(object? parameter)
        {
            return IsAdmin;
        }

        private void UpdateCurrentViewTitle()
        {
            CurrentViewTitle = CurrentViewViewModel switch
            {
                SalleViewModel => "Gestion des Salles",
                MatiereViewModel => "Gestion des Matières",
                ProfesseurViewModel => "Gestion des Professeurs",
                EtudiantViewModel => "Gestion des Étudiants",
                GroupeViewModel => "Gestion des Groupes",
                InscriptionViewModel => "Gestion des Inscriptions",
                PlanningViewModel => "Planification des Séances",
                GestionUtilisateursViewModel => "Gestion des Utilisateurs",
                PaiementViewModel => "Gestion Paiement",
                _ => "Tableau de bord"
            };
        }

        private async Task NavigateTo(object? viewModel)
        {
            if (viewModel == null) return;

            CurrentViewViewModel = viewModel;

            // Rafraîchir les données du ViewModel qui vient d'être activé
            try
            {
                switch (viewModel)
                {
                    case SalleViewModel svm:
                        await svm.LoadSallesAsync();
                        break;
                    case MatiereViewModel mvm:
                        await mvm.LoadMatieresAsync();
                        break;
                    case ProfesseurViewModel pvm:
                        await pvm.LoadProfesseursAsync();
                        break;
                    case EtudiantViewModel evm:
                        await evm.LoadEtudiantsAsync();
                        break;
                    case GroupeViewModel gvm:
                        await gvm.LoadAllInitialDataAsync();
                        break;
                    case InscriptionViewModel ivm:
                        await ivm.LoadAllDataAsync();
                        break;
                    case PlanningViewModel planvm:
                        await planvm.LoadAllInitialDataAsync();
                        break;
                    case GestionUtilisateursViewModel guvm:
                        await guvm.LoadUtilisateursAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                // Gérer les erreurs de chargement
                System.Windows.MessageBox.Show(
                    $"Erreur lors du chargement des données :\n{ex.Message}",
                    "Erreur de chargement",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}