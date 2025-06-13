using System;
using System.Windows;
using System.Windows.Controls;
using centre_soutien.ViewModels;
using centre_soutien.Models;

namespace centre_soutien.Views
{
    public partial class PlanningView : UserControl
    {
        public PlanningView()
        {
            InitializeComponent();
        }

        private void AddSlotButton_Click(object sender, RoutedEventArgs e)
        {
            if (FormPanel.Visibility == Visibility.Collapsed)
            {
                // Afficher le formulaire et masquer le tableau
                FormPanel.Visibility = Visibility.Visible;
                DataGridPanel.Visibility = Visibility.Collapsed; // Nouveau : masquer le tableau
                AddSlotButton.Content = "❌ Fermer le Formulaire";
        
                // Vider le formulaire pour un nouveau créneau
                if (DataContext is PlanningViewModel viewModel)
                {
                    viewModel.ClearFormCommand?.Execute(null);
                }
            }
            else
            {
                // Masquer le formulaire et réafficher le tableau
                FormPanel.Visibility = Visibility.Collapsed;
                DataGridPanel.Visibility = Visibility.Visible; // Nouveau : réafficher le tableau
                AddSlotButton.Content = "➕ Nouveau Créneau";
            }
        }

        // Annuler et fermer le formulaire
       // Annuler et fermer le formulaire
private void CancelSlotButton_Click(object sender, RoutedEventArgs e)
{
    FormPanel.Visibility = Visibility.Collapsed;
    DataGridPanel.Visibility = Visibility.Visible; // Nouveau : réafficher le tableau
    AddSlotButton.Content = "➕ Nouveau Créneau";
    
    // Vider le formulaire
    if (DataContext is PlanningViewModel viewModel)
    {
        viewModel.ClearFormCommand?.Execute(null);
    }
}

// Enregistrer le créneau (ajouter ou modifier selon le contexte)
private void SaveSlotButton_Click(object sender, RoutedEventArgs e)
{
    if (DataContext is PlanningViewModel viewModel)
    {
        // Si un créneau est sélectionné, on modifie, sinon on ajoute
        if (viewModel.SelectedCreneauForEditDelete != null)
        {
            // Mode modification
            if (viewModel.UpdateCreneauCommand.CanExecute(null))
            {
                viewModel.UpdateCreneauCommand.Execute(null);
                
                // Fermer le formulaire et réafficher le tableau après modification réussie
                FormPanel.Visibility = Visibility.Collapsed;
                DataGridPanel.Visibility = Visibility.Visible; // Nouveau : réafficher le tableau
                AddSlotButton.Content = "➕ Nouveau Créneau";
            }
            else
            {
                MessageBox.Show("⚠️ Veuillez remplir tous les champs obligatoires pour modifier le créneau.", 
                              "Champs manquants", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
            }
        }
        else
        {
            // Mode ajout
            if (viewModel.AddCreneauCommand.CanExecute(null))
            {
                viewModel.AddCreneauCommand.Execute(null);
                
                // Fermer le formulaire et réafficher le tableau après ajout réussi
                FormPanel.Visibility = Visibility.Collapsed;
                DataGridPanel.Visibility = Visibility.Visible; // Nouveau : réafficher le tableau
                AddSlotButton.Content = "➕ Nouveau Créneau";
            }
            else
            {
                MessageBox.Show("⚠️ Veuillez remplir tous les champs obligatoires :\n\n" +
                              "• Groupe\n" +
                              "• Salle\n" +
                              "• Date de début de validité\n" +
                              "• Heures de début et fin", 
                              "Champs manquants", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
            }
        }
    }
}

        // Modifier un créneau - charger les données dans le formulaire
        private void EditSlotButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is SeancePlanning seance)
            {
                if (DataContext is PlanningViewModel viewModel)
                {
                    // Sélectionner le créneau (cela va automatiquement remplir le formulaire)
                    viewModel.SelectedCreneauForEditDelete = seance;
                    
                    // Afficher le formulaire
                    FormPanel.Visibility = Visibility.Visible;
                    AddSlotButton.Content = "❌ Fermer le Formulaire";
                }
            }
        }

        // Activer/Désactiver un créneau
        private void ToggleSlotButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is SeancePlanning seance)
            {
                if (DataContext is PlanningViewModel viewModel)
                {
                    string action = seance.EstActif ? "désactiver" : "activer";
                    string actionPast = seance.EstActif ? "désactivé" : "activé";
                    
                    var result = MessageBox.Show(
                        $"⚠️ CONFIRMATION ⚠️\n\n" +
                        $"Voulez-vous {action} ce créneau ?\n\n" +
                        $"📅 {seance.JourSemaine} de {seance.HeureDebut} à {seance.HeureFin}\n" +
                        $"📚 Groupe : {seance.Groupe?.NomDescriptifGroupe}\n" +
                        $"🏫 Salle : {seance.Salle?.NomOuNumeroSalle}\n\n" +
                        $"ℹ️ {(seance.EstActif ? "Le créneau sera suspendu." : "Le créneau redeviendra actif.")}", 
                        $"Confirmation - {action}", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Sélectionner le créneau
                        viewModel.SelectedCreneauForEditDelete = seance;
                        
                        if (seance.EstActif)
                        {
                            // Désactiver
                            if (viewModel.DeactivateCreneauCommand.CanExecute(null))
                            {
                                viewModel.DeactivateCreneauCommand.Execute(null);
                                MessageBox.Show($"✅ Le créneau a été {actionPast} avec succès.", 
                                              "Opération réussie", 
                                              MessageBoxButton.OK, 
                                              MessageBoxImage.Information);
                            }
                        }
                        else
                        {
                            // Activer
                            if (viewModel.ActivateCreneauCommand.CanExecute(null))
                            {
                                viewModel.ActivateCreneauCommand.Execute(null);
                                MessageBox.Show($"✅ Le créneau a été {actionPast} avec succès.", 
                                              "Opération réussie", 
                                              MessageBoxButton.OK, 
                                              MessageBoxImage.Information);
                            }
                        }
                    }
                }
            }
        }

        // Supprimer un créneau
        private void DeleteSlotButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is SeancePlanning seance)
            {
                var result = MessageBox.Show(
                    $"⚠️ ATTENTION - SUPPRESSION DÉFINITIVE ⚠️\n\n" +
                    $"Êtes-vous sûr de vouloir supprimer définitivement ce créneau ?\n\n" +
                    $"📅 {seance.JourSemaine} de {seance.HeureDebut} à {seance.HeureFin}\n" +
                    $"📚 Groupe : {seance.Groupe?.NomDescriptifGroupe}\n" +
                    $"🎓 Matière : {seance.Groupe?.Matiere?.NomMatiere}\n" +
                    $"🏫 Salle : {seance.Salle?.NomOuNumeroSalle}\n\n" +
                    $"❌ Cette action est IRRÉVERSIBLE !\n" +
                    $"💡 Conseil : Utilisez plutôt la désactivation pour suspendre temporairement.", 
                    "Confirmation de suppression définitive", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (DataContext is PlanningViewModel viewModel)
                    {
                        // Sélectionner le créneau et le supprimer
                        viewModel.SelectedCreneauForEditDelete = seance;
                        
                        if (viewModel.DeleteCreneauCommand.CanExecute(null))
                        {
                            viewModel.DeleteCreneauCommand.Execute(null);
                            MessageBox.Show($"✅ Le créneau a été supprimé définitivement.", 
                                          "Suppression réussie", 
                                          MessageBoxButton.OK, 
                                          MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("❌ Impossible de supprimer ce créneau.\nIl pourrait être lié à des séances existantes.", 
                                          "Suppression impossible", 
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
            if (DataContext is PlanningViewModel viewModel)
            {
                // Échap pour fermer le formulaire
                if (e.Key == System.Windows.Input.Key.Escape && FormPanel.Visibility == Visibility.Visible)
                {
                    CancelSlotButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                
                // Ctrl+N pour nouveau créneau
                if (e.Key == System.Windows.Input.Key.N && 
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
                {
                    AddSlotButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                
                // Ctrl+S pour sauvegarder (si le formulaire est ouvert)
                if (e.Key == System.Windows.Input.Key.S && 
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control &&
                    FormPanel.Visibility == Visibility.Visible)
                {
                    SaveSlotButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }

                // F5 pour actualiser
                if (e.Key == System.Windows.Input.Key.F5)
                {
                    if (viewModel.LoadInitialDataCommand.CanExecute(null))
                    {
                        viewModel.LoadInitialDataCommand.Execute(null);
                    }
                    e.Handled = true;
                }
            }
        }
    }
}