using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Collections.Generic;
using centre_soutien.ViewModels;
using centre_soutien.Models;

namespace centre_soutien.Views
{
    /// <summary>
    /// Logique d'interaction pour GestionPaiementsView.xaml
    /// </summary>
    public partial class GestionPaiementsView : UserControl
    {
        private PaiementViewModel ViewModel => (PaiementViewModel)DataContext;

        public GestionPaiementsView()
        {
            InitializeComponent();
            
            // Initialiser le DataContext si ce n'est pas fait automatiquement
            if (DataContext == null)
            {
                DataContext = new PaiementViewModel();
            }
        }

        /// <summary>
        /// Gestionnaire pour le changement d'état des cases à cocher des échéances
        /// Déclenche le recalcul automatique du montant
        /// </summary>
        private void EcheanceCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            // Recalculer automatiquement le montant quand une échéance est cochée/décochée
            if (ViewModel?.CalculerMontantAutoCommand?.CanExecute(null) == true)
            {
                ViewModel.CalculerMontantAutoCommand.Execute(null);
            }
        }

        /// <summary>
        /// Réinitialise le formulaire de paiement
        /// </summary>
        private void ResetFormulaire_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null) return;

            // Réinitialiser les valeurs du formulaire
            ViewModel.MontantTotal = 0;
            ViewModel.NotesPaiement = string.Empty;
            ViewModel.DatePaiement = System.DateTime.Today;

            // Décocher toutes les échéances
            if (ViewModel.EcheancesEtudiant != null)
            {
                foreach (var echeance in ViewModel.EcheancesEtudiant)
                {
                    echeance.EstSelectionne = false;
                    echeance.MontantAPayer = echeance.EcheanceInfo.MontantRestant;
                }
            }

            ViewModel.StatusMessage = "Formulaire réinitialisé.";
        }

        /// <summary>
        /// Gestionnaire pour la sélection d'un étudiant dans la ComboBox
        /// </summary>
        private void EtudiantComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Le ViewModel gère déjà le changement via le binding
            // Cette méthode peut être utilisée pour des actions supplémentaires si nécessaire
        }

        /// <summary>
        /// Gestionnaire pour le changement de montant à payer dans le DataGrid
        /// </summary>
        private void MontantAPayer_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Recalculer le montant total quand un montant individuel change
            if (ViewModel?.CalculerMontantAutoCommand?.CanExecute(null) == true)
            {
                ViewModel.CalculerMontantAutoCommand.Execute(null);
            }
        }

        /// <summary>
        /// Validation en temps réel pour s'assurer que seuls les nombres sont entrés
        /// </summary>
        private void MontantTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Autoriser uniquement les chiffres, la virgule et le point
            if (!IsNumericInput(e.Text))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Vérifie si l'entrée est numérique
        /// </summary>
        private bool IsNumericInput(string input)
        {
            foreach (char c in input)
            {
                if (!char.IsDigit(c) && c != '.' && c != ',' && c != '-')
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gestionnaire pour l'événement de chargement du contrôle
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Charger les données initiales si nécessaire
            if (ViewModel?.LoadDataCommand?.CanExecute(null) == true)
            {
                ViewModel.LoadDataCommand.Execute(null);
            }
        }

        /// <summary>
        /// Gestionnaire pour le double-clic sur une ligne d'historique
        /// Permet de voir les détails d'un paiement
        /// </summary>
        private void HistoriquePaiement_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Paiement paiement)
            {
                // Afficher les détails du paiement dans une MessageBox
                string details = $"Détails du paiement:\n\n" +
                               $"Date: {paiement.DatePaiement}\n" +
                               $"Montant: {paiement.MontantTotalRecuTransaction:C}\n" +
                               $"Notes: {paiement.Notes ?? "Aucune note"}\n" +
                               $"ID: {paiement.IDPaiement}";

                MessageBox.Show(details, "Détails du paiement", 
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Gestionnaire pour la touche Entrée dans le champ de montant
        /// </summary>
        private void MontantTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                // Déclencher l'enregistrement du paiement si tout est valide
                if (ViewModel?.EnregistrerPaiementCommand?.CanExecute(null) == true)
                {
                    ViewModel.EnregistrerPaiementCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Gestionnaire pour la validation des données avant enregistrement
        /// </summary>
        private void ValiderDonnees()
        {
            if (ViewModel == null) return;

            var erreurs = new List<string>();

            // Vérifier qu'un étudiant est sélectionné
            if (ViewModel.SelectedEtudiant == null)
            {
                erreurs.Add("Veuillez sélectionner un étudiant.");
            }

            // Vérifier qu'au moins une échéance est sélectionnée
            if (ViewModel.EcheancesEtudiant?.Any(e => e.EstSelectionne) != true)
            {
                erreurs.Add("Veuillez sélectionner au moins une échéance.");
            }

            // Vérifier que le montant est positif
            if (ViewModel.MontantTotal <= 0)
            {
                erreurs.Add("Le montant doit être supérieur à zéro.");
            }

            // Afficher les erreurs s'il y en a
            if (erreurs.Any())
            {
                string message = "Erreurs de validation:\n\n" + string.Join("\n", erreurs);
                MessageBox.Show(message, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Gestionnaire pour l'événement avant l'enregistrement
        /// </summary>
        private void BeforeEnregistrerPaiement()
        {
            // Valider les données
            ValiderDonnees();

            // Demander confirmation si le ViewModel existe
            if (ViewModel?.SelectedEtudiant != null)
            {
                var result = MessageBox.Show(
                    $"Confirmer l'enregistrement du paiement de {ViewModel.MontantTotalFormate} pour {ViewModel.SelectedEtudiant.NomComplet}?",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Actualise l'affichage après une modification
        /// </summary>
        private void RefreshDisplay()
        {
            // Forcer la mise à jour de l'interface via le recalcul automatique
            if (ViewModel?.CalculerMontantAutoCommand?.CanExecute(null) == true)
            {
                ViewModel.CalculerMontantAutoCommand.Execute(null);
            }
        }
    }
}