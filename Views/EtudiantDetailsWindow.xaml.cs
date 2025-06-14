using System;
using System.Diagnostics;
using System.Windows;
using centre_soutien.Models;
using centre_soutien.ViewModels;

namespace centre_soutien.Views
{
    /// <summary>
    /// Logique d'interaction pour EtudiantDetailleWindow.xaml
    /// </summary>
    public partial class EtudiantDetailleWindow : Window
    {
        public EtudiantDetailsViewModel ViewModel { get; private set; }

        public EtudiantDetailleWindow(Etudiant etudiant)
        {
            InitializeComponent();
            
            // Initialiser le ViewModel avec l'étudiant sélectionné
            ViewModel = new EtudiantDetailsViewModel(etudiant);
            DataContext = ViewModel;
        }

        /// <summary>
        /// Gestionnaire pour le bouton Appeler
        /// </summary>
        private void AppelerButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel?.EtudiantActuel?.Telephone != null)
            {
                try
                {
                    // Essayer d'ouvrir l'application téléphone par défaut
                    // Vous pouvez adapter cette partie selon vos besoins
                    string telephone = ViewModel.EtudiantActuel.Telephone;
                    
                    // Nettoyer le numéro de téléphone (retirer les espaces, tirets, etc.)
                    telephone = telephone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
                    
                    MessageBox.Show($"Appel en cours vers : {telephone}", 
                                   "Appel", 
                                   MessageBoxButton.OK, 
                                   MessageBoxImage.Information);
                    
                    // Alternative : ouvrir avec l'URI tel:
                    // Process.Start(new ProcessStartInfo($"tel:{telephone}") { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Impossible d'initier l'appel : {ex.Message}", 
                                   "Erreur", 
                                   MessageBoxButton.OK, 
                                   MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Aucun numéro de téléphone disponible pour cet étudiant.", 
                               "Information", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Gestionnaire pour le bouton Fermer
        /// </summary>
        private void FermerButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Gestionnaire pour rafraîchir les données
        /// </summary>
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                await ViewModel.RefreshDataAsync();
            }
        }

        /// <summary>
        /// Gestionnaire pour l'événement de fermeture de la fenêtre
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // Nettoyer les ressources si nécessaire
            base.OnClosed(e);
        }
    }
}