using System.Windows;
using centre_soutien.Services;
using centre_soutien.Views; // Pour la fenêtre de login

namespace centre_soutien
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // DataContext = new ViewModels.MainViewModel(); // Fait dans le XAML
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Demander confirmation
            var result = MessageBox.Show(
                "🚪 DÉCONNEXION\n\n" +
                "Êtes-vous sûr de vouloir vous déconnecter ?\n\n" +
                "Toutes les données non sauvegardées seront perdues.",
                "Confirmation de déconnexion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                // Nettoyer la session utilisateur
                CurrentUserSession.ClearCurrentUser();

                // Créer et afficher la fenêtre de login
                var loginView = new LoginView();
                loginView.Show();

                // Fermer la fenêtre principale
                this.Close();
            }
        }

        // Optionnel : gérer la fermeture de la fenêtre
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show(
                "❌ FERMETURE DE L'APPLICATION\n\n" +
                "Êtes-vous sûr de vouloir quitter l'application ?\n\n" +
                "Toutes les données non sauvegardées seront perdues.",
                "Confirmation de fermeture",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true; // Annuler la fermeture
            }
            else
            {
                // Nettoyer la session avant de fermer
                CurrentUserSession.ClearCurrentUser();
            }

            base.OnClosing(e);
        }
    }
}