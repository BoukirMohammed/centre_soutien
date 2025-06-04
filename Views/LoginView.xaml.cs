using centre_soutien.ViewModels; // Pour LoginViewModel
using System.ComponentModel;
using System.Windows;

namespace centre_soutien.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            // Le DataContext est défini en XAML.
            // On s'abonne à l'événement Loaded pour attacher le handler PropertyChanged
            // une fois que le DataContext est réellement disponible.
            Loaded += LoginView_Loaded;
        }

        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LoginViewModel.IsLoginSuccessful))
            {
                if (DataContext is LoginViewModel vm && vm.IsLoginSuccessful)
                {
                    DialogResult = true; // Ferme la fenêtre et indique le succès
                }
            }
        }

        // Important de se désabonner pour éviter les fuites de mémoire
        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.PropertyChanged -= ViewModel_PropertyChanged;
            }
            base.OnClosed(e);
        }
    }
}