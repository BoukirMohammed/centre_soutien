using System;
using System.Windows;
using System.Windows.Controls;
using centre_soutien.ViewModels;
using centre_soutien.Models;

namespace centre_soutien.Views
{
    /// <summary>
    /// Logique d'interaction pour GestionSallesView.xaml
    /// </summary>
    public partial class GestionSallesView : UserControl
    {
        public GestionSallesView()
        {
            InitializeComponent();
        }

        // Afficher/masquer le formulaire pour ajouter une salle
        private void AddSalleButton_Click(object sender, RoutedEventArgs e)
        {
            if (FormPanel.Visibility == Visibility.Collapsed)
            {
                // Afficher le formulaire et masquer le tableau
                FormPanel.Visibility = Visibility.Visible;
                DataGridPanel.Visibility = Visibility.Collapsed;
                AddSalleButton.Content = "❌ Fermer le Formulaire";
                
                // Vider le formulaire pour une nouvelle salle
                if (DataContext is SalleViewModel viewModel)
                {
                    viewModel.ClearFormCommand?.Execute(null);
                }
            }
            else
            {
                // Masquer le formulaire et réafficher le tableau
                FormPanel.Visibility = Visibility.Collapsed;
                DataGridPanel.Visibility = Visibility.Visible;
                AddSalleButton.Content = "➕ Nouvelle Salle";
            }
        }

        // Annuler et fermer le formulaire
        private void CancelSalleButton_Click(object sender, RoutedEventArgs e)
        {
            FormPanel.Visibility = Visibility.Collapsed;
            DataGridPanel.Visibility = Visibility.Visible;
            AddSalleButton.Content = "➕ Nouvelle Salle";
            
            // Vider le formulaire
            if (DataContext is SalleViewModel viewModel)
            {
                viewModel.ClearFormCommand?.Execute(null);
            }
        }

        // Enregistrer la salle (ajouter ou modifier selon le contexte)
        private void SaveSalleButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SalleViewModel viewModel)
            {
                // Si une salle est sélectionnée, on modifie, sinon on ajoute
                if (viewModel.SelectedSalle != null)
                {
                    // Mode modification
                    if (viewModel.UpdateSalleCommand.CanExecute(null))
                    {
                        viewModel.UpdateSalleCommand.Execute(null);
                        
                        // Fermer le formulaire après modification réussie
                        FormPanel.Visibility = Visibility.Collapsed;
                        DataGridPanel.Visibility = Visibility.Visible;
                        AddSalleButton.Content = "➕ Nouvelle Salle";
                    }
                    else
                    {
                        MessageBox.Show("⚠️ Veuillez remplir le nom de la salle pour pouvoir la modifier.", 
                                      "Champs manquants", 
                                      MessageBoxButton.OK, 
                                      MessageBoxImage.Warning);
                    }
                }
                else
                {
                    // Mode ajout
                    if (viewModel.AddSalleCommand.CanExecute(null))
                    {
                        viewModel.AddSalleCommand.Execute(null);
                        
                        // Fermer le formulaire après ajout réussi
                        FormPanel.Visibility = Visibility.Collapsed;
                        DataGridPanel.Visibility = Visibility.Visible;
                        AddSalleButton.Content = "➕ Nouvelle Salle";
                    }
                    else
                    {
                        MessageBox.Show("⚠️ Veuillez remplir le nom de la salle pour pouvoir l'ajouter.", 
                                      "Champ obligatoire manquant", 
                                      MessageBoxButton.OK, 
                                      MessageBoxImage.Warning);
                    }
                }
            }
        }

        // Modifier une salle - charger les données dans le formulaire
        private void EditSalleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Salle salle)
            {
                if (DataContext is SalleViewModel viewModel)
                {
                    // Sélectionner la salle (cela va automatiquement remplir le formulaire)
                    viewModel.SelectedSalle = salle;
                    
                    // Afficher le formulaire
                    FormPanel.Visibility = Visibility.Visible;
                    DataGridPanel.Visibility = Visibility.Collapsed;
                    AddSalleButton.Content = "❌ Fermer le Formulaire";
                }
            }
        }

        // Archiver/Désarchiver une salle
        private void ArchiveSalleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Salle salle)
            {
                if (DataContext is SalleViewModel viewModel)
                {
                    string action = salle.EstArchivee ? "désarchiver" : "archiver";
                    string actionPast = salle.EstArchivee ? "désarchivée" : "archivée";
                    
                    var result = MessageBox.Show(
                        $"⚠️ CONFIRMATION ⚠️\n\n" +
                        $"Voulez-vous {action} cette salle ?\n\n" +
                        $"🏫 Salle : {salle.NomOuNumeroSalle}\n" +
                        $"👥 Capacité : {salle.Capacite?.ToString() ?? "Non définie"}\n" +
                        $"📝 Description : {(string.IsNullOrEmpty(salle.Description) ? "Aucune" : salle.Description)}\n\n" +
                        $"ℹ️ {(salle.EstArchivee ? "La salle redeviendra disponible." : "La salle ne sera plus disponible pour de nouveaux créneaux.")}", 
                        $"Confirmation - {action}", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Sélectionner la salle
                        viewModel.SelectedSalle = salle;
                        
                        // Archiver (la même commande gère l'archivage et le désarchivage)
                        if (viewModel.ArchiveSalleCommand.CanExecute(null))
                        {
                            viewModel.ArchiveSalleCommand.Execute(null);
                            
                            // Message de confirmation sera géré par le StatusMessage du ViewModel
                            // Pas besoin d'afficher un MessageBox supplémentaire ici
                        }
                        else
                        {
                            MessageBox.Show("❌ Impossible d'archiver cette salle.\nElle pourrait être utilisée dans des créneaux actifs.", 
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
            if (DataContext is SalleViewModel viewModel)
            {
                // Échap pour fermer le formulaire
                if (e.Key == System.Windows.Input.Key.Escape && FormPanel.Visibility == Visibility.Visible)
                {
                    CancelSalleButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                
                // Ctrl+N pour nouvelle salle
                if (e.Key == System.Windows.Input.Key.N && 
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
                {
                    AddSalleButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                
                // Ctrl+S pour sauvegarder (si le formulaire est ouvert)
                if (e.Key == System.Windows.Input.Key.S && 
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control &&
                    FormPanel.Visibility == Visibility.Visible)
                {
                    SaveSalleButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }

                // F5 pour actualiser
                if (e.Key == System.Windows.Input.Key.F5)
                {
                    if (viewModel.LoadSallesCommand.CanExecute(null))
                    {
                        viewModel.LoadSallesCommand.Execute(null);
                    }
                    e.Handled = true;
                }
            }
        }
    }
}