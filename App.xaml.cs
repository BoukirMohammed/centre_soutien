// Dans App.xaml.cs
using centre_soutien.ViewModels;
using centre_soutien.Views;
using centre_soutien.Services; // <<<< AJOUTER CE USING
using System.Windows;
using System; // Nécessaire pour Exception
using centre_soutien.Helpers; // Plus besoin pour le hachage temporaire ici

namespace centre_soutien
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e) // Peut rester void si pas d'await direct ici
        {
            base.OnStartup(e);

            // --- Supprimer le code temporaire pour générer le hachage si encore présent ---

            // 1. Créer et afficher la fenêtre de connexion
            var loginView = new LoginView();
            // Le DataContext (LoginViewModel) est déjà défini dans le XAML de LoginView

            // Afficher la fenêtre de connexion en mode dialogue
            bool? loginResult = loginView.ShowDialog();

            // 2. Vérifier le résultat de la connexion
            if (loginResult == true)
            {
                // La connexion a réussi. Récupérer l'utilisateur.
                if (loginView.DataContext is LoginViewModel loginVM && loginVM.AuthenticatedUser != null)
                {
                    // Stocker l'utilisateur connecté pour un accès global
                    CurrentUserSession.SetCurrentUser(loginVM.AuthenticatedUser);
                    
                    try
                    {
                        // Afficher la fenêtre principale
                        var mainWindow = new MainWindow();
                        
                        // Définir MainWindow comme la fenêtre principale de l'application AVANT de l'afficher
                        this.MainWindow = mainWindow;
                        mainWindow.Show();
              
                        
                        // Optionnel: Fermer la fenêtre de login explicitement si elle n'est pas déjà fermée
                        // par le DialogResult. ShowDialog() la ferme normalement.
                        // loginView.Close(); // Normalement pas nécessaire avec ShowDialog et DialogResult
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Une erreur critique est survenue lors du démarrage de la fenêtre principale : {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", 
                            "Erreur de Démarrage", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Error);
                        Shutdown(); // Fermer l'application en cas d'erreur grave au démarrage de MainWindow
                    }
                }
                else
                {
                    // Ce cas indique un problème dans LoginViewModel (IsLoginSuccessful=true mais AuthenticatedUser=null)
                    MessageBox.Show(
                        "Erreur d'authentification inattendue après une connexion apparemment réussie. L'application va se fermer.", 
                        "Erreur d'Authentification Interne", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                    Shutdown();
                }
            }
            else
            {
                // La connexion a échoué, a été annulée, ou la fenêtre de login a été fermée autrement
                // (DialogResult est false ou null). L'application doit se terminer.
                Shutdown();
            }
        }
    }
}