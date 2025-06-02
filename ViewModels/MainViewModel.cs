using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks; // Nécessaire pour Task
using System.Windows.Input;
using centre_soutien.Commands; // Assure-toi que ce using pointe vers ta classe RelayCommand

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
        public PlanningViewModel PlanningVM { get; private set; } // <<< MODIFIÉ/AJOUTÉ ICI (Nom de propriété)

        // Commandes pour la navigation
        public ICommand ShowSallesViewCommand { get; }
        public ICommand ShowMatieresViewCommand { get; }
        public ICommand ShowProfesseursViewCommand { get; }
        public ICommand ShowEtudiantsViewCommand { get; }
        public ICommand ShowGroupesViewCommand { get; }
        public ICommand ShowInscriptionsViewCommand { get; }
        public ICommand ShowPlanningViewCommand { get; } // <<< AJOUTÉ ICI

        public MainViewModel()
        {
            // Initialiser les ViewModels enfants
            SalleVM = new SalleViewModel();
            MatiereVM = new MatiereViewModel();
            ProfesseurVM = new ProfesseurViewModel();
            EtudiantVM = new EtudiantViewModel();
            GroupeVM = new GroupeViewModel();
            InscriptionVM = new InscriptionViewModel();
            PlanningVM = new PlanningViewModel(); // <<< MODIFIÉ/AJOUTÉ ICI (Initialisation)

            // Définir les commandes de navigation
            ShowSallesViewCommand = new RelayCommand(async param => await NavigateTo(SalleVM));
            ShowMatieresViewCommand = new RelayCommand(async param => await NavigateTo(MatiereVM));
            ShowProfesseursViewCommand = new RelayCommand(async param => await NavigateTo(ProfesseurVM));
            ShowEtudiantsViewCommand = new RelayCommand(async param => await NavigateTo(EtudiantVM));
            ShowGroupesViewCommand = new RelayCommand(async param => await NavigateTo(GroupeVM));
            ShowInscriptionsViewCommand = new RelayCommand(async param => await NavigateTo(InscriptionVM));
            ShowPlanningViewCommand = new RelayCommand(async param => await NavigateTo(PlanningVM)); // <<< AJOUTÉ ICI

            // Définir la vue par défaut au démarrage de l'application
            _ = NavigateTo(SalleVM); // Affiche la gestion des salles par défaut
        }

        private async Task NavigateTo(object? viewModel)
        {
            CurrentViewViewModel = viewModel;

            // Rafraîchir les données du ViewModel qui vient d'être activé
            if (viewModel is SalleViewModel svm)
            {
                await svm.LoadSallesAsync(); 
            }
            else if (viewModel is MatiereViewModel mvm)
            {
                await mvm.LoadMatieresAsync(); 
            }
            else if (viewModel is ProfesseurViewModel pvm)
            {
                await pvm.LoadProfesseursAsync(); 
            }
            else if (viewModel is EtudiantViewModel evm)
            {
                await evm.LoadEtudiantsAsync(); 
            }
            else if (viewModel is GroupeViewModel gvm)
            {
                await gvm.LoadAllDataAsync(); 
            }
            else if (viewModel is InscriptionViewModel ivm)
            {
                await ivm.LoadAllDataAsync(); 
            }
            else if (viewModel is PlanningViewModel planvm) // <<< AJOUTÉ ICI
            {
                await planvm.LoadAllInitialDataAsync(); // Assure-toi que cette méthode est public async Task dans PlanningViewModel
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}