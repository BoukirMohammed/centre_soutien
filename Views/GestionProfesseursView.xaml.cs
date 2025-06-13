using System;
using System.Windows;
using System.Windows.Controls;
using centre_soutien.ViewModels;
using centre_soutien.Models;

namespace centre_soutien.Views
{
    /// <summary>
    /// Logique d'interaction pour GestionProfesseursView.xaml
    /// </summary>
    public partial class GestionProfesseursView : UserControl
    {
        public GestionProfesseursView()
        {
            InitializeComponent();
            // PAS de DataContext = new ProfesseurViewModel(); ici
        }

        // Afficher/masquer le formulaire pour ajouter un professeur
        private void AddProfesseurButton_Click(object sender, RoutedEventArgs e)
        {
            if (FormPanel.Visibility == Visibility.Collapsed)
            {
                // Afficher le formulaire et masquer le tableau
                FormPanel.Visibility = Visibility.Visible;
                DataGridPanel.Visibility = Visibility.Collapsed;
                AddProfesseurButton.Content = "❌ Fermer le Formulaire";
                
                // Vider le formulaire pour un nouveau professeur
                if (DataContext is ProfesseurViewModel viewModel)
                {
                    viewModel.ClearFormCommand?.Execute(null);
                }
            }
            else
            {
                // Masquer le formulaire et réafficher le tableau
                FormPanel.Visibility = Visibility.Collapsed;
                DataGridPanel.Visibility = Visibility.Visible;
                AddProfesseurButton.Content = "➕ Nouveau Professeur";
            }
        }

        // Annuler et fermer le formulaire
        private void CancelProfesseurButton_Click(object sender, RoutedEventArgs e)
        {
            FormPanel.Visibility = Visibility.Collapsed;
            DataGridPanel.Visibility = Visibility.Visible;
            AddProfesseurButton.Content = "➕ Nouveau Professeur";
            
            // Vider le formulaire
            if (DataContext is ProfesseurViewModel viewModel)
            {
                viewModel.ClearFormCommand?.Execute(null);
            }
        }

        // Enregistrer le professeur (ajouter ou modifier selon le contexte)
        private void SaveProfesseurButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProfesseurViewModel viewModel)
            {
                // Si un professeur est sélectionné, on modifie, sinon on ajoute
                if (viewModel.SelectedProfesseur != null)
                {
                    // Mode modification
                    if (viewModel.UpdateProfesseurCommand.CanExecute(null))
                    {
                        viewModel.UpdateProfesseurCommand.Execute(null);
                        
                        // Fermer le formulaire après modification réussie
                        FormPanel.Visibility = Visibility.Collapsed;
                        DataGridPanel.Visibility = Visibility.Visible;
                        AddProfesseurButton.Content = "➕ Nouveau Professeur";
                    }
                    else
                    {
                        MessageBox.Show("⚠️ Veuillez remplir le nom et le prénom pour pouvoir modifier le professeur.", 
                                      "Champs manquants", 
                                      MessageBoxButton.OK, 
                                      MessageBoxImage.Warning);
                    }
                }
                else
                {
                    // Mode ajout
                    if (viewModel.AddProfesseurCommand.CanExecute(null))
                    {
                        viewModel.AddProfesseurCommand.Execute(null);
                        
                        // Fermer le formulaire après ajout réussi
                        FormPanel.Visibility = Visibility.Collapsed;
                        DataGridPanel.Visibility = Visibility.Visible;
                        AddProfesseurButton.Content = "➕ Nouveau Professeur";
                    }
                    else
                    {
                        MessageBox.Show("⚠️ Veuillez remplir les champs obligatoires :\n\n" +
                                      "• Nom\n" +
                                      "• Prénom", 
                                      "Champs obligatoires manquants", 
                                      MessageBoxButton.OK, 
                                      MessageBoxImage.Warning);
                    }
                }
            }
        }

        // Modifier un professeur - charger les données dans le formulaire
        private void EditProfesseurButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Professeur professeur)
            {
                if (DataContext is ProfesseurViewModel viewModel)
                {
                    // Sélectionner le professeur (cela va automatiquement remplir le formulaire)
                    viewModel.SelectedProfesseur = professeur;
                    
                    // Afficher le formulaire
                    FormPanel.Visibility = Visibility.Visible;
                    DataGridPanel.Visibility = Visibility.Collapsed;
                    AddProfesseurButton.Content = "❌ Fermer le Formulaire";
                }
            }
        }

        // Gérer les matières d'un professeur
        private void GererMatieresButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Professeur professeur)
            {
                if (DataContext is ProfesseurViewModel viewModel)
                {
                    // Sélectionner le professeur
                    viewModel.SelectedProfesseur = professeur;
                    
                    // Ouvrir la fenêtre de gestion des matières
                    if (viewModel.OpenGestionMatieresProfesseurCommand.CanExecute(null))
                    {
                        viewModel.OpenGestionMatieresProfesseurCommand.Execute(null);
                    }
                }
            }
        }

        // Archiver un professeur
        private void ArchiveProfesseurButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Professeur professeur)
            {
                if (DataContext is ProfesseurViewModel viewModel)
                {
                    string action = professeur.EstArchive ? "désarchiver" : "archiver";
                    string actionPast = professeur.EstArchive ? "désarchivé" : "archivé";
                    
                    var result = MessageBox.Show(
                        $"⚠️ CONFIRMATION ⚠️\n\n" +
                        $"Voulez-vous {action} ce professeur ?\n\n" +
                        $"👨‍🏫 Professeur : {professeur.Prenom} {professeur.Nom}\n" +
                        $"📞 Téléphone : {professeur.Telephone ?? "Non renseigné"}\n" +
                        $"📝 Notes : {(string.IsNullOrEmpty(professeur.Notes) ? "Aucune" : professeur.Notes)}\n\n" +
                        $"ℹ️ {(professeur.EstArchive ? "Le professeur redeviendra disponible." : "Le professeur ne sera plus disponible pour de nouveaux cours.")}", 
                        $"Confirmation - {action}", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Sélectionner le professeur
                        viewModel.SelectedProfesseur = professeur;
                        
                        // Archiver (la commande gère l'archivage et le désarchivage)
                        if (viewModel.ArchiveProfesseurCommand.CanExecute(null))
                        {
                            viewModel.ArchiveProfesseurCommand.Execute(null);
                            
                            // Le message de confirmation sera géré par le StatusMessage du ViewModel
                        }
                        else
                        {
                            MessageBox.Show("❌ Impossible d'archiver ce professeur.\nIl pourrait être assigné à des cours actifs.", 
                                          "Archivage impossible", 
                                          MessageBoxButton.OK, 
                                          MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }

        // Gestion des raccourcis clavier
        private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is ProfesseurViewModel viewModel)
            {
                // Échap pour fermer le formulaire
                if (e.Key == System.Windows.Input.Key.Escape && FormPanel.Visibility == Visibility.Visible)
                {
                    CancelProfesseurButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                
                // Ctrl+N pour nouveau professeur
                if (e.Key == System.Windows.Input.Key.N && 
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
                {
                    AddProfesseurButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                
                // Ctrl+S pour sauvegarder (si le formulaire est ouvert)
                if (e.Key == System.Windows.Input.Key.S && 
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control &&
                    FormPanel.Visibility == Visibility.Visible)
                {
                    SaveProfesseurButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }

                // F5 pour actualiser
                if (e.Key == System.Windows.Input.Key.F5)
                {
                    if (viewModel.LoadProfesseursCommand.CanExecute(null))
                    {
                        viewModel.LoadProfesseursCommand.Execute(null);
                    }
                    e.Handled = true;
                }

                // Ctrl+M pour gérer les matières (si un professeur est sélectionné)
                if (e.Key == System.Windows.Input.Key.M && 
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control &&
                    viewModel.SelectedProfesseur != null)
                {
                    if (viewModel.OpenGestionMatieresProfesseurCommand.CanExecute(null))
                    {
                        viewModel.OpenGestionMatieresProfesseurCommand.Execute(null);
                    }
                    e.Handled = true;
                }
            }
        }
    }
}