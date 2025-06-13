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

        // Afficher les détails d'un étudiant
        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Etudiant etudiant)
            {
                // Créer un message avec tous les détails
                string details = $"📋 DÉTAILS DE L'ÉTUDIANT\n" +
                               $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n" +
                               $"👤 Nom complet: {etudiant.Nom} {etudiant.Prenom}\n" +
                               $"🎂 Date de naissance: {(etudiant.DateNaissance != null ? DateTime.Parse(etudiant.DateNaissance).ToString("dd/MM/yyyy") : "Non renseignée")}\n" +
                               $"📞 Téléphone: {etudiant.Telephone ?? "Non renseigné"}\n" +
                               $"🏫 Lycée: {etudiant.Lycee ?? "Non renseigné"}\n" +
                               $"🏠 Adresse: {etudiant.Adresse ?? "Non renseignée"}\n" +
                               $"📝 Notes: {etudiant.Notes ?? "Aucune note"}\n" +
                               $"📅 Inscrit le: {(etudiant.DateInscriptionSysteme != null ? DateTime.Parse(etudiant.DateInscriptionSysteme).ToString("dd/MM/yyyy") : "Non renseigné")}\n" +
                               $"📁 Statut: {(etudiant.EstArchive ? "Archivé" : "Actif")}";

                MessageBox.Show(details, "Informations détaillées", MessageBoxButton.OK, MessageBoxImage.Information);
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