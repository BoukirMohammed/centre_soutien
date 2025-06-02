using centre_soutien.ViewModels; // Pour AddInscriptionViewModel
using System.ComponentModel;      // Pour PropertyChangedEventArgs
using System.Windows;

namespace centre_soutien.Views
{
    public partial class AddInscriptionWindow : Window
    {
        public AddInscriptionWindow()
        {
            InitializeComponent();

            // Le DataContext sera défini de l'extérieur (par InscriptionViewModel)
            // Mais on peut s'abonner à PropertyChanged pour fermer la fenêtre
            Loaded += AddInscriptionWindow_Loaded;
        }

        private void AddInscriptionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddInscriptionViewModel vm)
            {
                vm.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AddInscriptionViewModel.InscriptionReussie))
            {
                if (DataContext is AddInscriptionViewModel vm && vm.InscriptionReussie)
                {
                    DialogResult = true; // Signale que l'opération a réussi
                    // La fenêtre se fermera automatiquement car DialogResult est défini
                }
            }
        }

        // Désabonnement pour éviter les fuites de mémoire
        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is AddInscriptionViewModel vm)
            {
                vm.PropertyChanged -= ViewModel_PropertyChanged;
            }
            base.OnClosed(e);
        }
    }
}