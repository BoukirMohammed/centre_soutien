using centre_soutien.ViewModels;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace centre_soutien.Views
{
    public partial class AddInscriptionWindow : Window
    {
        public AddInscriptionWindow()
        {
            InitializeComponent();

            // Le DataContext sera défini de l'extérieur (par InscriptionViewModel)
            // Mais on peut s'abonner à PropertyChanged pour fermer la fenêtre
            Loaded += AddInscriptionWindow_Loaded;
        }

        private void AddInscriptionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddInscriptionViewModel vm)
            {
                vm.PropertyChanged += ViewModel_PropertyChanged;
                
                // Focus sur le premier champ (ComboBox étudiant)
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var firstComboBox = this.FindName("EtudiantComboBox") as System.Windows.Controls.ComboBox;
                    firstComboBox?.Focus();
                }));
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AddInscriptionViewModel.InscriptionReussie))
            {
                if (DataContext is AddInscriptionViewModel vm && vm.InscriptionReussie)
                {
                    DialogResult = true; // Signale que l'opération a réussi
                    // La fenêtre se fermera automatiquement car DialogResult est défini
                }
            }
            
            // Gérer le focus lors de la création d'un nouvel étudiant
            if (e.PropertyName == nameof(AddInscriptionViewModel.IsAddingNewStudent))
            {
                if (DataContext is AddInscriptionViewModel viewModel && viewModel.IsAddingNewStudent)
                {
                    // Donner le focus au premier champ du formulaire d'ajout d'étudiant
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var nomTextBox = this.FindName("NomTextBox") as System.Windows.Controls.TextBox;
                        nomTextBox?.Focus();
                    }));
                }
            }
        }

        // Désabonnement pour éviter les fuites de mémoire
        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is AddInscriptionViewModel vm)
            {
                vm.PropertyChanged -= ViewModel_PropertyChanged;
            }
            base.OnClosed(e);
        }

        // Gestion des raccourcis clavier
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is AddInscriptionViewModel viewModel)
            {
                // Échap pour fermer
                if (e.Key == Key.Escape)
                {
                    // Si on est en train de créer un étudiant, d'abord annuler la création
                    if (viewModel.IsAddingNewStudent)
                    {
                        if (viewModel.CancelNewStudentCommand.CanExecute(null))
                        {
                            viewModel.CancelNewStudentCommand.Execute(null);
                        }
                    }
                    else
                    {
                        DialogResult = false;
                    }
                    e.Handled = true;
                }
                
                // Ctrl+S pour sauvegarder
                if (e.Key == Key.S && 
                    (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    if (viewModel.IsAddingNewStudent)
                    {
                        // Sauvegarder le nouvel étudiant
                        if (viewModel.CreateNewStudentCommand.CanExecute(null))
                        {
                            viewModel.CreateNewStudentCommand.Execute(null);
                        }
                    }
                    else
                    {
                        // Sauvegarder l'inscription
                        if (viewModel.InscrireCommand.CanExecute(null))
                        {
                            viewModel.InscrireCommand.Execute(null);
                        }
                    }
                    e.Handled = true;
                }

                // Ctrl+N pour nouveau étudiant
                if (e.Key == Key.N && 
                    (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    if (viewModel.OpenAddStudentCommand.CanExecute(null))
                    {
                        viewModel.OpenAddStudentCommand.Execute(null);
                    }
                    e.Handled = true;
                }

                // F5 pour recharger les données
                if (e.Key == Key.F5)
                {
                    if (viewModel.LoadDataCommand.CanExecute(null))
                    {
                        viewModel.LoadDataCommand.Execute(null);
                    }
                    e.Handled = true;
                }
            }
        }

        // Validation avant fermeture
        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult != true && DataContext is AddInscriptionViewModel vm)
            {
                // Vérifier s'il y a des données non sauvegardées
                bool hasUnsavedData = vm.SelectedEtudiant != null || 
                                     vm.SelectedGroupe != null || 
                                     vm.PrixConvenuInput.HasValue || 
                                     vm.JourEcheanceInput.HasValue ||
                                     vm.IsAddingNewStudent ||
                                     !string.IsNullOrWhiteSpace(vm.NewStudentNom) ||
                                     !string.IsNullOrWhiteSpace(vm.NewStudentPrenom);

                if (hasUnsavedData)
                {
                    var result = MessageBox.Show(
                        "⚠️ Vous avez des données non sauvegardées.\n\n" +
                        "Êtes-vous sûr de vouloir fermer sans enregistrer ?", 
                        "Données non sauvegardées", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.No)
                    {
                        e.Cancel = true; // Annuler la fermeture
                        return;
                    }
                }
            }

            base.OnClosing(e);
        }

        // Méthodes utilitaires pour améliorer l'UX
        private void SetFocusToNextControl(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Déplacer le focus au contrôle suivant
                var request = new TraversalRequest(FocusNavigationDirection.Next);
                if (sender is System.Windows.Controls.Control control)
                {
                    control.MoveFocus(request);
                    e.Handled = true;
                }
            }
        }

        // Méthode pour valider les données en temps réel
        private void ValidateInputs()
        {
            if (DataContext is AddInscriptionViewModel vm)
            {
                // Validation du prix
                if (vm.PrixConvenuInput.HasValue && vm.PrixConvenuInput < 0)
                {
                    vm.StatusMessage = "⚠️ Le prix ne peut pas être négatif.";
                }
                
                // Validation du jour d'échéance
                if (vm.JourEcheanceInput.HasValue && 
                    (vm.JourEcheanceInput < 1 || vm.JourEcheanceInput > 31))
                {
                    vm.StatusMessage = "⚠️ Le jour d'échéance doit être entre 1 et 31.";
                }
            }
        }

        // Méthode pour gérer les erreurs de validation
        private void HandleValidationError(object sender, System.Windows.Controls.ValidationErrorEventArgs e)
        {
            if (DataContext is AddInscriptionViewModel vm)
            {
                if (e.Action == System.Windows.Controls.ValidationErrorEventAction.Added)
                {
                    vm.StatusMessage = $"⚠️ Erreur de validation : {e.Error.ErrorContent}";
                }
                else if (e.Action == System.Windows.Controls.ValidationErrorEventAction.Removed)
                {
                    // Effacer le message d'erreur si la validation passe
                    if (string.IsNullOrEmpty(vm.StatusMessage) || vm.StatusMessage.StartsWith("⚠️ Erreur de validation"))
                    {
                        vm.StatusMessage = string.Empty;
                    }
                }
            }
        }

        // Gestion spéciale pour la navigation par Tab
        private void HandleTabNavigation(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab && DataContext is AddInscriptionViewModel vm)
            {
                // Si on est dans le formulaire de création d'étudiant et qu'on arrive au dernier champ
                if (vm.IsAddingNewStudent && sender is System.Windows.Controls.TextBox textBox)
                {
                    if (textBox.Name == "NotesTextBox") // Dernier champ du formulaire étudiant
                    {
                        // Donner le focus au bouton "Créer Étudiant"
                        var createButton = this.FindName("CreateStudentButton") as System.Windows.Controls.Button;
                        createButton?.Focus();
                        e.Handled = true;
                    }
                }
            }
        }

        // Méthode pour afficher des conseils contextuels
        private void ShowContextualHelp(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Control control)
            {
                string helpText = control.Name switch
                {
                    "EtudiantComboBox" => "💡 Sélectionnez un étudiant existant ou cliquez sur 'Nouvel Étudiant' pour en créer un.",
                    "GroupeComboBox" => "💡 Choisissez le groupe dans lequel inscrire l'étudiant. Le prix sera pré-rempli automatiquement.",
                    "PrixTextBox" => "💡 Vous pouvez modifier le prix standard si un tarif spécial a été convenu avec l'étudiant.",
                    "JourEcheanceTextBox" => "💡 Définissez le jour du mois où les paiements seront dus (généralement entre 1 et 31).",
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(helpText) && DataContext is AddInscriptionViewModel vm)
                {
                    vm.StatusMessage = helpText;
                }
            }
        }

        // Animation pour montrer/cacher le formulaire de création d'étudiant
        private void AnimateStudentFormVisibility(bool show)
        {
            var studentForm = this.FindName("StudentFormBorder") as System.Windows.Controls.Border;
            if (studentForm != null)
            {
                var storyboard = new System.Windows.Media.Animation.Storyboard();
                var animation = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = show ? 0 : 1,
                    To = show ? 1 : 0,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new System.Windows.Media.Animation.QuadraticEase()
                };

                System.Windows.Media.Animation.Storyboard.SetTarget(animation, studentForm);
                System.Windows.Media.Animation.Storyboard.SetTargetProperty(animation, 
                    new PropertyPath(System.Windows.UIElement.OpacityProperty));
                
                storyboard.Children.Add(animation);
                storyboard.Begin();
            }
        }
    }
}