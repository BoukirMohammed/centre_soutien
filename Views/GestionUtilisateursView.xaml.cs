using System;
using System.Windows;
using System.Windows.Controls;
using centre_soutien.ViewModels;
using centre_soutien.Models;

namespace centre_soutien.Views
{
    /// <summary>
    /// Logique d'interaction pour GestionUtilisateursView.xaml
    /// </summary>
    public partial class GestionUtilisateursView : UserControl
    {
        public GestionUtilisateursView()
        {
            InitializeComponent();
        }

        // Afficher/masquer le formulaire pour ajouter un utilisateur
        private void AddUtilisateurButton_Click(object sender, RoutedEventArgs e)
        {
            if (FormPanel.Visibility == Visibility.Collapsed)
            {
                // Afficher le formulaire et masquer le tableau
                FormPanel.Visibility = Visibility.Visible;
                DataGridPanel.Visibility = Visibility.Collapsed;
                AddUtilisateurButton.Content = "❌ Fermer le Formulaire";
                
                // Préparer pour un nouvel utilisateur
                if (DataContext is GestionUtilisateursViewModel viewModel)
                {
                    viewModel.PrepareAddUtilisateurCommand?.Execute(null);
                }
                
                // Vider le champ mot de passe
                UserPasswordBox.Clear();
            }
            else
            {
                // Masquer le formulaire et réafficher le tableau
                FormPanel.Visibility = Visibility.Collapsed;
                DataGridPanel.Visibility = Visibility.Visible;
                AddUtilisateurButton.Content = "➕ Nouvel Utilisateur";
                
                // Annuler l'édition
                if (DataContext is GestionUtilisateursViewModel viewModel)
                {
                    viewModel.CancelEditCommand?.Execute(null);
                }
            }
        }

        // Annuler et fermer le formulaire
        private void CancelUtilisateurButton_Click(object sender, RoutedEventArgs e)
        {
            FormPanel.Visibility = Visibility.Collapsed;
            DataGridPanel.Visibility = Visibility.Visible;
            AddUtilisateurButton.Content = "➕ Nouvel Utilisateur";
            
            // Annuler l'édition
            if (DataContext is GestionUtilisateursViewModel viewModel)
            {
                viewModel.CancelEditCommand?.Execute(null);
            }
            
            // Vider le champ mot de passe
            UserPasswordBox.Clear();
        }

        // Enregistrer l'utilisateur (ajouter ou modifier selon le contexte)
        private void SaveUtilisateurButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is GestionUtilisateursViewModel viewModel)
            {
                // Vérifier si on a un mot de passe pour un nouvel utilisateur
                if (viewModel.SelectedUtilisateur == null && UserPasswordBox.SecurePassword.Length == 0)
                {
                    MessageBox.Show("⚠️ Le mot de passe est obligatoire pour un nouvel utilisateur.", 
                                  "Mot de passe requis", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Warning);
                    return;
                }

                if (viewModel.SaveUtilisateurCommand.CanExecute(UserPasswordBox))
                {
                    viewModel.SaveUtilisateurCommand.Execute(UserPasswordBox);
                    
                    // Si l'opération s'est bien passée (pas d'erreur dans StatusMessage), fermer le formulaire
                    if (!viewModel.StatusMessage.StartsWith("Erreur") && !viewModel.StatusMessage.Contains("requis"))
                    {
                        FormPanel.Visibility = Visibility.Collapsed;
                        DataGridPanel.Visibility = Visibility.Visible;
                        AddUtilisateurButton.Content = "➕ Nouvel Utilisateur";
                        UserPasswordBox.Clear();
                    }
                }
                else
                {
                    MessageBox.Show("⚠️ Veuillez remplir tous les champs obligatoires :\n\n" +
                                  "• Login\n" +
                                  "• Nom complet\n" +
                                  "• Rôle\n" +
                                  "• Mot de passe (pour nouvel utilisateur)", 
                                  "Champs obligatoires manquants", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Warning);
                }
            }
        }

        // Changer le mot de passe de l'utilisateur sélectionné
        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is GestionUtilisateursViewModel viewModel)
            {
                if (viewModel.SelectedUtilisateur == null)
                {
                    MessageBox.Show("⚠️ Veuillez sélectionner un utilisateur pour changer son mot de passe.", 
                                  "Aucun utilisateur sélectionné", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Warning);
                    return;
                }

                if (UserPasswordBox.SecurePassword.Length == 0)
                {
                    MessageBox.Show("⚠️ Veuillez saisir un nouveau mot de passe.", 
                                  "Mot de passe requis", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"⚠️ CONFIRMATION ⚠️\n\n" +
                    $"Voulez-vous changer le mot de passe de l'utilisateur ?\n\n" +
                    $"👤 Utilisateur : {viewModel.SelectedUtilisateur.Login}\n" +
                    $"👥 Nom : {viewModel.SelectedUtilisateur.NomComplet}\n\n" +
                    $"🔐 Cette action modifiera définitivement le mot de passe.", 
                    "Confirmation changement mot de passe", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (viewModel.ChangePasswordCommand.CanExecute(UserPasswordBox))
                    {
                        viewModel.ChangePasswordCommand.Execute(UserPasswordBox);
                        UserPasswordBox.Clear();
                    }
                }
            }
        }

        // Modifier un utilisateur - charger les données dans le formulaire
        private void EditUtilisateurButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Utilisateur utilisateur)
            {
                if (DataContext is GestionUtilisateursViewModel viewModel)
                {
                    // Sélectionner l'utilisateur (cela va automatiquement remplir le formulaire)
                    viewModel.SelectedUtilisateur = utilisateur;
                    viewModel.IsEditing = true;
                    
                    // Afficher le formulaire
                    FormPanel.Visibility = Visibility.Visible;
                    DataGridPanel.Visibility = Visibility.Collapsed;
                    AddUtilisateurButton.Content = "❌ Fermer le Formulaire";
                    
                    // Vider le champ mot de passe (pour modification, on ne charge pas l'ancien)
                    UserPasswordBox.Clear();
                }
            }
        }

        // Activer/Désactiver un utilisateur
        private void ToggleUtilisateurButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Utilisateur utilisateur)
            {
                if (DataContext is GestionUtilisateursViewModel viewModel)
                {
                    string action = utilisateur.EstActif ? "désactiver" : "activer";
                    string actionPast = utilisateur.EstActif ? "désactivé" : "activé";
                    
                    // Vérification spéciale pour les administrateurs
                    if (utilisateur.Role == "Admin" && utilisateur.EstActif)
                    {
                        var result = MessageBox.Show(
                            $"⚠️ ATTENTION - DÉSACTIVATION ADMINISTRATEUR ⚠️\n\n" +
                            $"Vous êtes sur le point de désactiver un administrateur :\n\n" +
                            $"👤 Login : {utilisateur.Login}\n" +
                            $"👥 Nom : {utilisateur.NomComplet}\n\n" +
                            $"⚠️ Assurez-vous qu'il reste au moins un autre administrateur actif !\n" +
                            $"Continuer ?", 
                            "Confirmation - Désactivation Administrateur", 
                            MessageBoxButton.YesNo, 
                            MessageBoxImage.Warning);

                        if (result != MessageBoxResult.Yes) return;
                    }
                    else
                    {
                        var result = MessageBox.Show(
                            $"⚠️ CONFIRMATION ⚠️\n\n" +
                            $"Voulez-vous {action} cet utilisateur ?\n\n" +
                            $"👤 Login : {utilisateur.Login}\n" +
                            $"👥 Nom : {utilisateur.NomComplet}\n" +
                            $"🔑 Rôle : {utilisateur.Role}\n\n" +
                            $"ℹ️ {(utilisateur.EstActif ? "L'utilisateur ne pourra plus se connecter." : "L'utilisateur pourra de nouveau se connecter.")}", 
                            $"Confirmation - {action}", 
                            MessageBoxButton.YesNo, 
                            MessageBoxImage.Question);

                        if (result != MessageBoxResult.Yes) return;
                    }

                    // Sélectionner l'utilisateur et exécuter la commande
                    viewModel.SelectedUtilisateur = utilisateur;
                    if (viewModel.ToggleActivationCommand.CanExecute(null))
                    {
                        viewModel.ToggleActivationCommand.Execute(null);
                    }
                }
            }
        }

        // Gestion des raccourcis clavier
        private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is GestionUtilisateursViewModel viewModel)
            {
                // Échap pour fermer le formulaire
                if (e.Key == System.Windows.Input.Key.Escape && FormPanel.Visibility == Visibility.Visible)
                {
                    CancelUtilisateurButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                
                // Ctrl+N pour nouvel utilisateur
                if (e.Key == System.Windows.Input.Key.N && 
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
                {
                    AddUtilisateurButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                
                // Ctrl+S pour sauvegarder (si le formulaire est ouvert)
                if (e.Key == System.Windows.Input.Key.S && 
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control &&
                    FormPanel.Visibility == Visibility.Visible)
                {
                    SaveUtilisateurButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }

                // F5 pour actualiser
                if (e.Key == System.Windows.Input.Key.F5)
                {
                    if (viewModel.LoadUtilisateursCommand.CanExecute(null))
                    {
                        viewModel.LoadUtilisateursCommand.Execute(null);
                    }
                    e.Handled = true;
                }

                // Ctrl+P pour changer le mot de passe (si un utilisateur est sélectionné et formulaire ouvert)
                if (e.Key == System.Windows.Input.Key.P && 
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control &&
                    FormPanel.Visibility == Visibility.Visible &&
                    viewModel.SelectedUtilisateur != null)
                {
                    ChangePasswordButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
            }
        }
    }
}