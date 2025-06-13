using centre_soutien.ViewModels;
using System;
using System.ComponentModel;
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
                    // Afficher un message de succès avant de fermer
                    MessageBox.Show("✅ Inscription réussie !\n\nL'étudiant a été inscrit au groupe avec succès.", 
                                  "Inscription confirmée", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Information);
                    
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

        // Gestion des raccourcis clavier
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is AddInscriptionViewModel viewModel)
            {
                // Échap pour fermer
                if (e.Key == System.Windows.Input.Key.Escape)
                {
                    DialogResult = false;
                    e.Handled = true;
                }
                
                // Ctrl+S pour sauvegarder
                if (e.Key == System.Windows.Input.Key.S && 
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
                {
                    if (viewModel.InscrireCommand.CanExecute(null))
                    {
                        viewModel.InscrireCommand.Execute(null);
                    }
                    e.Handled = true;
                }
            }
        }

        // Validation avant fermeture (optionnel)
        protected override void OnClosing(CancelEventArgs e)
        {
            // Si l'utilisateur ferme la fenêtre sans sauvegarder et qu'il y a des données
            if (DialogResult != true && DataContext is AddInscriptionViewModel vm)
            {
                if (vm.SelectedEtudiant != null || vm.SelectedGroupe != null || 
                    vm.PrixConvenuInput.HasValue || vm.JourEcheanceInput.HasValue)
                {
                    var result = MessageBox.Show(
                        "⚠️ Vous avez des données non sauvegardées.\n\n" +
                        "Êtes-vous sûr de vouloir fermer sans enregistrer ?", 
                        "Données non sauvegardées", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.No)
                    {
                        e.Cancel = true; // Annuler la fermeture
                        return;
                    }
                }
            }

            base.OnClosing(e);
        }
    }
}