using System;
using System.Windows;
using System.Windows.Controls;
using centre_soutien.ViewModels;
using centre_soutien.Models;

namespace centre_soutien.Views
{
    public partial class GestionInscriptionsView : UserControl
    {
        public GestionInscriptionsView()
        {
            InitializeComponent();
        }

        // Ouvrir la fenêtre d'ajout d'inscription
        private void AddInscriptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InscriptionViewModel viewModel)
            {
                // Utiliser la commande existante du ViewModel
                if (viewModel.OpenAddInscriptionDialogCommand.CanExecute(null))
                {
                    viewModel.OpenAddInscriptionDialogCommand.Execute(null);
                }
                else
                {
                    MessageBox.Show("❌ Impossible d'ouvrir la fenêtre d'ajout d'inscription.\nVeuillez réessayer.", 
                                  "Erreur", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Warning);
                }
            }
        }

        // Afficher les détails d'une inscription
        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Inscription inscription)
            {
                // Helper pour formater les dates string
                string FormatDate(string? dateString)
                {
                    if (string.IsNullOrWhiteSpace(dateString))
                        return "Non spécifiée";
                    
                    if (DateTime.TryParse(dateString, out DateTime date))
                        return date.ToString("dd/MM/yyyy");
                    
                    return dateString;
                }

                // Créer un message avec tous les détails
                string details = $"📋 DÉTAILS DE L'INSCRIPTION\n" +
                               $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n" +
                               $"👤 Étudiant: {inscription.Etudiant?.NomComplet ?? "Non spécifié"}\n" +
                               $"📚 Groupe: {inscription.Groupe?.NomDescriptifGroupe ?? "Non spécifié"}\n" +
                               $"🎓 Matière: {inscription.Groupe?.Matiere?.NomMatiere ?? "Non spécifiée"}\n" +
                               $"👨‍🏫 Professeur: {inscription.Groupe?.Professeur?.NomComplet ?? "Non spécifié"}\n" +
                               $"💰 Prix convenu: {inscription.PrixConvenuMensuel:C} DH/mois\n" +
                               $"📅 Inscrit le: {FormatDate(inscription.DateInscription)}\n" +
                               $"📆 Jour d'échéance: {inscription.JourEcheanceMensuelle} du mois\n" +
                               $"📊 Statut: {(inscription.EstActif ? "✅ Active" : "❌ Inactive")}\n" +
                               $"📋 ID Inscription: {inscription.IDInscription}";

                if (!inscription.EstActif && !string.IsNullOrWhiteSpace(inscription.DateDesinscription))
                {
                    details += $"\n🚫 Désinscrit le: {FormatDate(inscription.DateDesinscription)}";
                }

                MessageBox.Show(details, "Informations détaillées", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Désinscrire un étudiant spécifique
        private void UnsubscribeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Inscription inscription)
            {
                if (!inscription.EstActif)
                {
                    MessageBox.Show("Cette inscription est déjà inactive.", 
                                  "Information", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"⚠️ CONFIRMATION DE DÉSINSCRIPTION ⚠️\n\n" +
                    $"Êtes-vous sûr de vouloir désinscrire :\n\n" +
                    $"👤 Étudiant : {inscription.Etudiant?.NomComplet}\n" +
                    $"📚 Groupe : {inscription.Groupe?.NomDescriptifGroupe}\n" +
                    $"🎓 Matière : {inscription.Groupe?.Matiere?.NomMatiere}\n\n" +
                    $"⚠️ Cette action désactivera l'inscription !", 
                    "Confirmation de désinscription", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (DataContext is InscriptionViewModel viewModel)
                    {
                        // Sélectionner l'inscription et exécuter la commande de désinscription
                        viewModel.SelectedInscription = inscription;
                        
                        if (viewModel.DesinscrireCommand.CanExecute(null))
                        {
                            viewModel.DesinscrireCommand.Execute(null);
                            MessageBox.Show($"✅ {inscription.Etudiant?.NomComplet} a été désinscrit avec succès.", 
                                          "Désinscription réussie", 
                                          MessageBoxButton.OK, 
                                          MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("❌ Impossible de désinscrire cet étudiant.\nVeuillez vérifier les droits d'accès.", 
                                          "Désinscription impossible", 
                                          MessageBoxButton.OK, 
                                          MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }

        // Désinscrire l'inscription sélectionnée (bouton principal)
        private void BulkUnsubscribeButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InscriptionViewModel viewModel)
            {
                if (viewModel.SelectedInscription == null)
                {
                    MessageBox.Show("⚠️ Veuillez d'abord sélectionner une inscription dans le tableau.", 
                                  "Aucune sélection", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Warning);
                    return;
                }

                if (!viewModel.SelectedInscription.EstActif)
                {
                    MessageBox.Show("Cette inscription est déjà inactive.", 
                                  "Information", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"⚠️ CONFIRMATION DE DÉSINSCRIPTION ⚠️\n\n" +
                    $"Êtes-vous sûr de vouloir désinscrire :\n\n" +
                    $"👤 {viewModel.SelectedInscription.Etudiant?.NomComplet}\n" +
                    $"📚 {viewModel.SelectedInscription.Groupe?.NomDescriptifGroupe}\n\n" +
                    $"⚠️ Cette action est irréversible !", 
                    "Confirmation de désinscription", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (viewModel.DesinscrireCommand.CanExecute(null))
                    {
                        viewModel.DesinscrireCommand.Execute(null);
                    }
                    else
                    {
                        MessageBox.Show("❌ Impossible de désinscrire cette inscription.", 
                                      "Erreur", 
                                      MessageBoxButton.OK, 
                                      MessageBoxImage.Error);
                    }
                }
            }
        }

        // Actualiser la liste des inscriptions
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InscriptionViewModel viewModel)
            {
                if (viewModel.LoadInscriptionsCommand.CanExecute(null))
                {
                    viewModel.LoadInscriptionsCommand.Execute(null);
                }
            }
        }

        // Gestion des raccourcis clavier (optionnel)
        private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is InscriptionViewModel viewModel)
            {
                // F5 pour actualiser
                if (e.Key == System.Windows.Input.Key.F5)
                {
                    RefreshButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                
                // Ctrl+N pour nouvelle inscription
                if (e.Key == System.Windows.Input.Key.N && 
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
                {
                    AddInscriptionButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                
                // Delete pour désinscrire la sélection
                if (e.Key == System.Windows.Input.Key.Delete && viewModel.SelectedInscription != null)
                {
                    BulkUnsubscribeButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
            }
        }
    }
}