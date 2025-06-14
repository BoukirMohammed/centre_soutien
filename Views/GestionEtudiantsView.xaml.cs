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

        // Supprimer un étudiant
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Etudiant etudiant)
            {
                var result = MessageBox.Show(
                    $"⚠️ ATTENTION ⚠️\n\n" +
                    $"Êtes-vous absolument sûr de vouloir supprimer l'étudiant :\n\n" +
                    $"👤 {etudiant.Nom} {etudiant.Prenom}\n" +
                    $"🏫 {etudiant.Lycee}\n\n" +
                    $"⚠️ Cette action est irréversible !", 
                    "Confirmation de suppression", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (DataContext is EtudiantViewModel viewModel)
                    {
                        // Pour l'instant, on utilise l'archivage comme suppression logique
                        // Tu peux créer une vraie commande de suppression plus tard
                        viewModel.SelectedEtudiant = etudiant;
                        
                        if (viewModel.ArchiveEtudiantCommand.CanExecute(null))
                        {
                            viewModel.ArchiveEtudiantCommand.Execute(null);
                            MessageBox.Show($"✅ L'étudiant {etudiant.Prenom} {etudiant.Nom} a été archivé (suppression logique).", 
                                          "Suppression réussie", 
                                          MessageBoxButton.OK, 
                                          MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("❌ Impossible de supprimer cet étudiant.\nIl pourrait avoir des inscriptions actives.", 
                                          "Suppression impossible", 
                                          MessageBoxButton.OK, 
                                          MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }
    }
}