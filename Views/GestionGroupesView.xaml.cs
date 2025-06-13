using System;
using System.Windows;
using System.Windows.Controls;
using centre_soutien.ViewModels;
using centre_soutien.Models;

namespace centre_soutien.Views
{
    public partial class GestionGroupesView : UserControl
    {
        public GestionGroupesView()
        {
            InitializeComponent();
        }

        // Afficher/masquer le formulaire pour ajouter un groupe
        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is GroupeViewModel viewModel)
            {
                // Vérifier qu'un professeur est sélectionné
                if (viewModel.SelectedProfesseurForFilter == null)
                {
                    MessageBox.Show("⚠️ Veuillez d'abord sélectionner un professeur dans la liste de gauche.", 
                                  "Professeur requis", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Warning);
                    return;
                }

                if (FormPanel.Visibility == Visibility.Collapsed)
                {
                    // Préparer le formulaire pour un nouveau groupe
                    viewModel.PrepareAddGroupeCommand?.Execute(null);
                    
                    FormPanel.Visibility = Visibility.Visible;
                    AddGroupButton.Content = "❌ Fermer le Formulaire";
                }
                else
                {
                    FormPanel.Visibility = Visibility.Collapsed;
                    AddGroupButton.Content = "➕ Nouveau Groupe";
                    
                    // Annuler l'édition en cours
                    viewModel.CancelEditCommand?.Execute(null);
                }
            }
        }

        // Annuler et fermer le formulaire
        private void CancelGroupButton_Click(object sender, RoutedEventArgs e)
        {
            FormPanel.Visibility = Visibility.Collapsed;
            AddGroupButton.Content = "➕ Nouveau Groupe";
            
            // Annuler l'édition via le ViewModel
            if (DataContext is GroupeViewModel viewModel)
            {
                viewModel.CancelEditCommand?.Execute(null);
            }
        }

        // Enregistrer le groupe (ajouter ou modifier selon le contexte)
        private void SaveGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is GroupeViewModel viewModel)
            {
                if (viewModel.SaveGroupeCommand.CanExecute(null))
                {
                    viewModel.SaveGroupeCommand.Execute(null);
                    
                    // Fermer le formulaire après sauvegarde réussie
                    FormPanel.Visibility = Visibility.Collapsed;
                    AddGroupButton.Content = "➕ Nouveau Groupe";
                }
                else
                {
                    MessageBox.Show("⚠️ Veuillez remplir tous les champs obligatoires :\n\n" +
                                  "• Nom du groupe\n" +
                                  "• Matière\n" +
                                  "• Professeur", 
                                  "Champs manquants", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Warning);
                }
            }
        }

        // Modifier un groupe - charger les données dans le formulaire
        private void EditGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Groupe groupe)
            {
                if (DataContext is GroupeViewModel viewModel)
                {
                    // Sélectionner le groupe (cela va automatiquement remplir le formulaire)
                    viewModel.SelectedGroupe = groupe;
                    
                    // Afficher le formulaire
                    FormPanel.Visibility = Visibility.Visible;
                    AddGroupButton.Content = "❌ Fermer le Formulaire";
                }
            }
        }

        // Afficher les étudiants inscrits dans le groupe
        private void StudentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Groupe groupe)
            {
                if (DataContext is GroupeViewModel viewModel)
                {
                    // Sélectionner le groupe et ouvrir la fenêtre des étudiants
                    viewModel.SelectedGroupe = groupe;
                    
                    if (viewModel.ShowEtudiantsInscritsCommand.CanExecute(null))
                    {
                        viewModel.ShowEtudiantsInscritsCommand.Execute(null);
                    }
                    else
                    {
                        MessageBox.Show("Impossible d'afficher les étudiants pour ce groupe.", 
                                      "Erreur", 
                                      MessageBoxButton.OK, 
                                      MessageBoxImage.Warning);
                    }
                }
            }
        }

        // Supprimer/Archiver un groupe
        private void DeleteGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Groupe groupe)
            {
                var result = MessageBox.Show(
                    $"⚠️ ATTENTION ⚠️\n\n" +
                    $"Êtes-vous sûr de vouloir archiver le groupe :\n\n" +
                    $"📚 {groupe.NomDescriptifGroupe}\n" +
                    $"🎓 Matière : {groupe.Matiere?.NomMatiere ?? "Non spécifiée"}\n" +
                    $"📝 Niveau : {groupe.Niveau ?? "Non spécifié"}\n\n" +
                    $"⚠️ Cette action archivera le groupe et pourrait affecter les inscriptions !", 
                    "Confirmation d'archivage", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (DataContext is GroupeViewModel viewModel)
                    {
                        // Sélectionner le groupe et l'archiver
                        viewModel.SelectedGroupe = groupe;
                        
                        if (viewModel.ArchiveGroupeCommand.CanExecute(null))
                        {
                            viewModel.ArchiveGroupeCommand.Execute(null);
                            MessageBox.Show($"✅ Le groupe '{groupe.NomDescriptifGroupe}' a été archivé avec succès.", 
                                          "Archivage réussi", 
                                          MessageBoxButton.OK, 
                                          MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("❌ Impossible d'archiver ce groupe.\n" +
                                          "Il pourrait avoir des inscriptions actives ou vous n'avez pas les droits nécessaires.", 
                                          "Archivage impossible", 
                                          MessageBoxButton.OK, 
                                          MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }

        // Gestion des raccourcis clavier (optionnel)
        private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is GroupeViewModel viewModel)
            {
                // Échap pour fermer le formulaire
                if (e.Key == System.Windows.Input.Key.Escape && FormPanel.Visibility == Visibility.Visible)
                {
                    CancelGroupButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                
                // Ctrl+N pour nouveau groupe
                if (e.Key == System.Windows.Input.Key.N && 
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
                {
                    AddGroupButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                
                // Ctrl+S pour sauvegarder (si le formulaire est ouvert)
                if (e.Key == System.Windows.Input.Key.S && 
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control &&
                    FormPanel.Visibility == Visibility.Visible)
                {
                    SaveGroupButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
            }
        }
    }
}