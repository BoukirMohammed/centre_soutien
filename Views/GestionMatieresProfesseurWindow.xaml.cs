using centre_soutien.ViewModels; // Pour GestionMatieresProfesseurViewModel
using System.ComponentModel;
using System.Windows;

namespace centre_soutien.Views
{
    public partial class GestionMatieresProfesseurWindow : Window
    {
        public GestionMatieresProfesseurWindow()
        {
            InitializeComponent();
            Loaded += GestionMatieresProfesseurWindow_Loaded;
        }

        private void GestionMatieresProfesseurWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is GestionMatieresProfesseurViewModel vm)
            {
                vm.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GestionMatieresProfesseurViewModel.ModificationsEnregistrees))
            {
                if (DataContext is GestionMatieresProfesseurViewModel vm && vm.ModificationsEnregistrees)
                {
                    DialogResult = true; // Signale le succès et ferme la fenêtre
                }
            }
        }

        protected override void OnClosed(System.EventArgs e)
        {
            if (DataContext is GestionMatieresProfesseurViewModel vm)
            {
                vm.PropertyChanged -= ViewModel_PropertyChanged;
            }
            base.OnClosed(e);
        }
    }
}