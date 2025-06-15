using System.Windows;
using centre_soutien.ViewModels;
using centre_soutien.Views; // Pour StatistiquesFinancieresGroupeWindow

namespace centre_soutien.Views
{
    /// <summary>
    /// Logique d'interaction pour EtudiantsInscritsWindow.xaml
    /// </summary>
    public partial class EtudiantsInscritsWindow : Window
    {
        public EtudiantsInscritsWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gestionnaire pour le bouton Actualiser
        /// </summary>
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EtudiantsInscritsViewModel viewModel)
            {
                await viewModel.RefreshDataAsync();
            }
        }

        /// <summary>
        /// Gestionnaire pour le bouton Fermer
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// NOUVEAU : Gestionnaire pour le bouton Statistiques Financières
        /// </summary>
        private void StatistiquesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Récupérer l'ID du groupe depuis le ViewModel
                if (DataContext is EtudiantsInscritsViewModel viewModel && 
                    viewModel.CurrentGroupe != null)
                {
                    var groupeId = viewModel.CurrentGroupe.IDGroupe;
                    
                    // Ouvrir la fenêtre des statistiques financières
                    var statistiquesWindow = new StatistiquesFinancieresGroupeWindow(groupeId)
                    {
                        Owner = this // Définir cette fenêtre comme parent
                    };

                    statistiquesWindow.ShowDialog(); // Afficher en mode modal
                }
                else
                {
                    MessageBox.Show(
                        "❌ Impossible d'accéder aux statistiques :\n\n" +
                        "Aucun groupe n'est actuellement sélectionné.",
                        "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    $"❌ Erreur lors de l'ouverture des statistiques :\n\n{ex.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        } 
    }
}