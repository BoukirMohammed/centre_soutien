using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks; // Nécessaire pour Task
using System.Windows.Input;
using centre_soutien.Commands; // Assure-toi que ce using pointe vers ta classe RelayCommand
using centre_soutien.Services; // Pour CurrentUserSession

namespace centre_soutien.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private object? _currentViewViewModel;
        public object? CurrentViewViewModel
        {
            get => _currentViewViewModel;
            set
            {
                _currentViewViewModel = value;
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
        public GestionUtilisateursViewModel? GestionUtilisateursVM { get; private set; } // <<<< DÉCLARÉ ET RENDU NULLABLE

        // Commandes pour la navigation
        public ICommand ShowSallesViewCommand { get; }
        public ICommand ShowMatieresViewCommand { get; }
        public ICommand ShowProfesseursViewCommand { get; }
        public ICommand ShowEtudiantsViewCommand { get; }
        public ICommand ShowGroupesViewCommand { get; }
        public ICommand ShowInscriptionsViewCommand { get; }
        public ICommand ShowPlanningViewCommand { get; }
        public ICommand? ShowGestionUtilisateursCommand { get; } // <<<< RENDU NULLABLE

        public bool IsAdmin { get; private set; }
        public bool IsSecretaire { get; private set; }
        public string? NomUtilisateurConnecte { get; private set; }

        public MainViewModel()
        {
            // Récupérer les infos de l'utilisateur connecté
            if (CurrentUserSession.IsUserLoggedIn)
            {
                IsAdmin = CurrentUserSession.IsAdmin;
                IsSecretaire = CurrentUserSession.IsSecretaire; // Tu peux aussi faire IsSecretaire = CurrentUserSession.CurrentUser?.Role == "Secretaire";
                NomUtilisateurConnecte = CurrentUserSession.CurrentUser?.NomComplet ?? CurrentUserSession.CurrentUser?.Login;
            }
            else
            {
                IsAdmin = false; 
                IsSecretaire = false;
            }

            // Initialiser les ViewModels enfants
            SalleVM = new SalleViewModel();
            MatiereVM = new MatiereViewModel();
            ProfesseurVM = new ProfesseurViewModel();
            EtudiantVM = new EtudiantViewModel();
            GroupeVM = new GroupeViewModel();
            InscriptionVM = new InscriptionViewModel();
            PlanningVM = new PlanningViewModel();
            
            if (IsAdmin) // Créer le ViewModel et la commande seulement si Admin
            {
                GestionUtilisateursVM = new GestionUtilisateursViewModel();
                ShowGestionUtilisateursCommand = new RelayCommand(async param => await NavigateTo(GestionUtilisateursVM), param => CanNavigateToGestionUtilisateurs(param));
            }


            // Définir les commandes de navigation
            // Pour les CanExecute, tu peux ajouter des prédicats si certaines vues ne sont pas accessibles à tous les rôles qui peuvent se logger
            ShowSallesViewCommand = new RelayCommand(async param => await NavigateTo(SalleVM));
            ShowMatieresViewCommand = new RelayCommand(async param => await NavigateTo(MatiereVM));
            ShowProfesseursViewCommand = new RelayCommand(async param => await NavigateTo(ProfesseurVM)); // Potentiellement Admin seulement ? Adapte le CanExecute.
            ShowEtudiantsViewCommand = new RelayCommand(async param => await NavigateTo(EtudiantVM));
            ShowGroupesViewCommand = new RelayCommand(async param => await NavigateTo(GroupeVM));
            ShowInscriptionsViewCommand = new RelayCommand(async param => await NavigateTo(InscriptionVM));
            ShowPlanningViewCommand = new RelayCommand(async param => await NavigateTo(PlanningVM));
            
            // Vue par défaut
            // Si l'utilisateur est loggué (ce qui devrait toujours être le cas si MainViewModel est créé après le login)
            if (CurrentUserSession.IsUserLoggedIn)
            {
                _ = NavigateTo(SalleVM); 
            }
            else
            {
                // Gérer le cas où MainViewModel est créé sans utilisateur loggué (ex: pour le designer XAML)
                // CurrentViewViewModel = null; // ou une vue "Bienvenue" / "Non connecté"
            }
        }
        
        // Prédicat pour la commande de gestion des utilisateurs
        private bool CanNavigateToGestionUtilisateurs(object? parameter)
        {
            return IsAdmin; // Seul l'admin peut accéder à cette vue
        }


        private async Task NavigateTo(object? viewModel)
        {
            if (viewModel == null) return; // Sécurité si un ViewModel conditionnel (comme GestionUtilisateursVM) est null

            CurrentViewViewModel = viewModel;

            // Rafraîchir les données du ViewModel qui vient d'être activé
            if (viewModel is SalleViewModel svm) { await svm.LoadSallesAsync(); }
            else if (viewModel is MatiereViewModel mvm) { await mvm.LoadMatieresAsync(); }
            else if (viewModel is ProfesseurViewModel pvm) { await pvm.LoadProfesseursAsync(); }
            else if (viewModel is EtudiantViewModel evm) { await evm.LoadEtudiantsAsync(); }
            else if (viewModel is GroupeViewModel gvm) { await gvm.LoadAllDataAsync(); }
            else if (viewModel is InscriptionViewModel ivm) { await ivm.LoadAllDataAsync(); }
            else if (viewModel is PlanningViewModel planvm) { await planvm.LoadAllInitialDataAsync(); }
            else if (viewModel is GestionUtilisateursViewModel guvm) // <<< AJOUTÉ ICI
            { 
                await guvm.LoadUtilisateursAsync(); // Assure-toi que cette méthode existe et est public async Task
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}