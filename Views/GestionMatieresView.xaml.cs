using System;
using System.Windows;
using System.Windows.Controls;
using centre_soutien.ViewModels;
using centre_soutien.Models;

namespace centre_soutien.Views
{
    public partial class GestionMatieresView : UserControl
    {
        public GestionMatieresView()
        {
            InitializeComponent();
        }

        // Afficher/masquer le formulaire
        private void AddSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (FormPanel.Visibility == Visibility.Collapsed)
            {
                FormPanel.Visibility = Visibility.Visible;
                AddSubjectButton.Content = "❌ Fermer le Formulaire";
                
                // Vider le formulaire pour une nouvelle matière
                if (DataContext is MatiereViewModel viewModel)
                {
                    viewModel.ClearFormCommand?.Execute(null);
                }
            }
            else
            {
                FormPanel.Visibility = Visibility.Collapsed;
                AddSubjectButton.Content = "➕ Ajouter une Matière";
            }
        }

        // Annuler et fermer le formulaire
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            FormPanel.Visibility = Visibility.Collapsed;
            AddSubjectButton.Content = "➕ Ajouter une Matière";
            
            // Vider le formulaire
            if (DataContext is MatiereViewModel viewModel)
            {
                viewModel.ClearFormCommand?.Execute(null);
            }
        }

        // Enregistrer (ajouter ou modifier selon le contexte)
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MatiereViewModel viewModel)
            {
                // Si une matière est sélectionnée, on modifie, sinon on ajoute
                if (viewModel.SelectedMatiere != null)
                {
                    // Mode modification
                    if (viewModel.UpdateMatiereCommand.CanExecute(null))
                    {
                        viewModel.UpdateMatiereCommand.Execute(null);
                        
                        // Fermer le formulaire après modification réussie
                        FormPanel.Visibility = Visibility.Collapsed;
                        AddSubjectButton.Content = "➕ Ajouter une Matière";
                    }
                }
                else
                {
                    // Mode ajout
                    if (viewModel.AddMatiereCommand.CanExecute(null))
                    {
                        viewModel.AddMatiereCommand.Execute(null);
                        
                        // Fermer le formulaire après ajout réussi
                        FormPanel.Visibility = Visibility.Collapsed;
                        AddSubjectButton.Content = "➕ Ajouter une Matière";
                    }
                }
            }
        }

        // Afficher les détails d'une matière
        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Matiere matiere)
            {
                // Créer un message avec tous les détails
                string details = $"📚 DÉTAILS DE LA MATIÈRE\n" +
                               $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n" +
                               $"📖 Nom: {matiere.NomMatiere}\n" +
                               $"💰 Prix standard: {matiere.PrixStandardMensuel:C} DH/mois\n" +
                               $"📝 Description: {matiere.Description ?? "Aucune description"}\n" +
                               $"📁 Statut: {(matiere.EstArchivee ? "Archivée" : "Active")}\n" +
                               $"🆔 ID: {matiere.IDMatiere}";

                MessageBox.Show(details, "Informations détaillées", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Modifier une matière - charger les données dans le formulaire
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Matiere matiere)
            {
                if (DataContext is MatiereViewModel viewModel)
                {
                    // Sélectionner la matière (cela va automatiquement remplir le formulaire)
                    viewModel.SelectedMatiere = matiere;
                    
                    // Afficher le formulaire
                    FormPanel.Visibility = Visibility.Visible;
                    AddSubjectButton.Content = "❌ Fermer le Formulaire";
                }
            }
        }

        // Supprimer une matière
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Matiere matiere)
            {
                var result = MessageBox.Show(
                    $"⚠️ ATTENTION ⚠️\n\n" +
                    $"Êtes-vous absolument sûr de vouloir supprimer la matière :\n\n" +
                    $"📚 {matiere.NomMatiere}\n" +
                    $"💰 {matiere.PrixStandardMensuel:C} DH/mois\n\n" +
                    $"⚠️ Cette action est irréversible !\n" +
                    $"⚠️ Tous les groupes et inscriptions liés seront affectés !", 
                    "Confirmation de suppression", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (DataContext is MatiereViewModel viewModel)
                    {
                        // Pour l'instant, on utilise l'archivage comme suppression logique
                        viewModel.SelectedMatiere = matiere;
                        
                        if (viewModel.ArchiveMatiereCommand.CanExecute(null))
                        {
                            viewModel.ArchiveMatiereCommand.Execute(null);
                            MessageBox.Show($"✅ La matière {matiere.NomMatiere} a été archivée (suppression logique).", 
                                          "Suppression réussie", 
                                          MessageBoxButton.OK, 
                                          MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("❌ Impossible de supprimer cette matière.\nElle pourrait avoir des groupes ou inscriptions actifs.", 
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