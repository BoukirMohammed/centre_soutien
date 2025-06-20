using System;
using System.Windows;
using System.Windows.Controls;
using centre_soutien.ViewModels;
using centre_soutien.Models;

namespace centre_soutien.Views
{
    public partial class GestionEtudiantsView : UserControl
    {
        public GestionEtudiantsView()
        {
            InitializeComponent();
        }

        // Enregistrer (ajouter ou modifier selon le contexte)
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EtudiantViewModel viewModel)
            {
                // Si un étudiant est sélectionné, on modifie, sinon on ajoute
                if (viewModel.SelectedEtudiant != null)
                {
                    // Mode modification
                    if (viewModel.UpdateEtudiantCommand.CanExecute(null))
                    {
                        viewModel.UpdateEtudiantCommand.Execute(null);

                        // Fermer le formulaire après modification réussie
                        FormPanel.Visibility = Visibility.Collapsed;
                        AddStudentButton.Content = "➕ Ajouter un Étudiant";
                    }
                }
                else
                {
                    // Mode ajout
                    if (viewModel.AddEtudiantCommand.CanExecute(null))
                    {
                        viewModel.AddEtudiantCommand.Execute(null);

                        // Fermer le formulaire après ajout réussi
                        FormPanel.Visibility = Visibility.Collapsed;
                        AddStudentButton.Content = "➕ Ajouter un Étudiant";
                    }
                }
            }
        }

        // Afficher/masquer le formulaire
        private void AddStudentButton_Click(object sender, RoutedEventArgs e)
        {
            if (FormPanel.Visibility == Visibility.Collapsed)
            {
                FormPanel.Visibility = Visibility.Visible;
                AddStudentButton.Content = "❌ Fermer le Formulaire";

                // Vider le formulaire pour un nouvel étudiant
                if (DataContext is EtudiantViewModel viewModel)
                {
                    viewModel.ClearFormCommand?.Execute(null);
                }
            }
            else
            {
                FormPanel.Visibility = Visibility.Collapsed;
                AddStudentButton.Content = "➕ Ajouter un Étudiant";
            }
        }

        // Annuler et fermer le formulaire
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            FormPanel.Visibility = Visibility.Collapsed;
            AddStudentButton.Content = "➕ Ajouter un Étudiant";

            // Vider le formulaire
            if (DataContext is EtudiantViewModel viewModel)
            {
                viewModel.ClearFormCommand?.Execute(null);
            }
        }

        /// <summary>
        /// Gestionnaire pour le bouton Détails - ouvre la fenêtre de détails de l'étudiant
        /// </summary>
        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Récupérer l'étudiant depuis le Tag du bouton
                if (sender is Button button && button.Tag is Etudiant etudiant)
                {
                    // Créer et ouvrir la fenêtre de détails
                    var detailsWindow = new EtudiantDetailleWindow(etudiant);
                    detailsWindow.Owner = Window.GetWindow(this); // Définir la fenêtre parent
                    detailsWindow.ShowDialog(); // Ouvrir en mode modal
                }
                else
                {
                    MessageBox.Show("Impossible de récupérer les informations de l'étudiant sélectionné.",
                        "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ouverture des détails : {ex.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // Modifier un étudiant - charger les données dans le formulaire
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Etudiant etudiant)
            {
                if (DataContext is EtudiantViewModel viewModel)
                {
                    // Sélectionner l'étudiant (cela va automatiquement remplir le formulaire grâce à ton ViewModel)
                    viewModel.SelectedEtudiant = etudiant;

                    // Afficher le formulaire
                    FormPanel.Visibility = Visibility.Visible;
                    AddStudentButton.Content = "❌ Fermer le Formulaire";
                }
            }
        }

        /// <summary>
        /// Archiver un étudiant (suppression logique) - accessible à tous les utilisateurs
        /// </summary>
        private void ArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Etudiant etudiant)
            {
                var result = MessageBox.Show(
                    $"📦 Confirmation d'archivage\n\n" +
                    $"Voulez-vous archiver l'étudiant :\n\n" +
                    $"👤 {etudiant.Nom} {etudiant.Prenom}\n" +
                    $"🏫 {etudiant.Lycee}\n\n" +
                    $"⚠️ L'étudiant sera masqué mais ses données seront conservées.",
                    "Archivage d'étudiant",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (DataContext is EtudiantViewModel viewModel)
                    {
                        viewModel.SelectedEtudiant = etudiant;

                        if (viewModel.ArchiveEtudiantCommand.CanExecute(null))
                        {
                            viewModel.ArchiveEtudiantCommand.Execute(null);
                            MessageBox.Show($"✅ L'étudiant {etudiant.Prenom} {etudiant.Nom} a été archivé avec succès.",
                                "Archivage réussi",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(
                                "❌ Impossible d'archiver cet étudiant.\nIl pourrait avoir des inscriptions actives.",
                                "Archivage impossible",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Supprimer définitivement un étudiant - accessible seulement aux administrateurs
        /// </summary>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Etudiant etudiant)
            {
                // Vérification des permissions
                if (DataContext is EtudiantViewModel viewModel && !viewModel.CanUserArchive)
                {
                    MessageBox.Show(
                        "❌ Vous n'avez pas les droits nécessaires pour supprimer définitivement un étudiant.",
                        "Accès refusé",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"⚠️ SUPPRESSION DÉFINITIVE ⚠️\n\n" +
                    $"ATTENTION : Cette action est IRRÉVERSIBLE !\n\n" +
                    $"Êtes-vous absolument sûr de vouloir supprimer définitivement :\n\n" +
                    $"👤 {etudiant.Nom} {etudiant.Prenom}\n" +
                    $"🏫 {etudiant.Lycee}\n\n" +
                    $"💀 Toutes les données seront perdues à jamais !",
                    "SUPPRESSION DÉFINITIVE",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                {
                    // Double confirmation pour les suppressions définitives
                    var secondConfirmation = MessageBox.Show(
                        "🚨 DERNIÈRE CHANCE 🚨\n\n" +
                        "Confirmez-vous vraiment cette suppression définitive ?\n\n" +
                        "Cette action ne peut PAS être annulée !",
                        "Confirmation finale",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Stop);

                    if (secondConfirmation == MessageBoxResult.Yes)
                    {
                        if (DataContext is EtudiantViewModel vm)
                        {
                            vm.SelectedEtudiant = etudiant;

                            if (vm.DeleteEtudiantCommand.CanExecute(null))
                            {
                                vm.DeleteEtudiantCommand.Execute(null);
                                MessageBox.Show(
                                    $"💀 L'étudiant {etudiant.Prenom} {etudiant.Nom} a été supprimé définitivement.",
                                    "Suppression réussie",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show(
                                    "❌ Impossible de supprimer cet étudiant.\nIl pourrait avoir des contraintes de base de données.",
                                    "Suppression impossible",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
        }
    }
}