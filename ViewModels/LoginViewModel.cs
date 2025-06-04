using centre_soutien.DataAccess; // Pour UtilisateurRepository
using centre_soutien.Helpers;    // Pour PasswordHasher
using centre_soutien.Models;     // Pour Utilisateur
using centre_soutien.Commands;   // Pour RelayCommand
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security; // Pour SecureString
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace centre_soutien.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly UtilisateurRepository _utilisateurRepository;

        private string _loginInput = string.Empty;

        public string LoginInput
        {
            get => _loginInput;
            set
            {
                _loginInput = value;
                OnPropertyChanged();
                (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        // Pour le PasswordBox, on ne stocke pas le mot de passe en clair.
        // La vue passera le SecureString ou le PasswordBox lui-même en paramètre de la commande.
        // Ou, pour une approche MVVM plus "pure", on pourrait utiliser une propriété jointe (attached property)
        // pour lier le PasswordBox.Text à une propriété SecureString dans le ViewModel.
        // Pour simplifier ici, on va supposer que le PasswordBox est passé en paramètre à la commande Execute.

        private string _errorMessage = string.Empty;

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoginSuccessful = false;

        public bool IsLoginSuccessful // La vue (fenêtre de login) observera cela pour se fermer
        {
            get => _isLoginSuccessful;
            private set
            {
                _isLoginSuccessful = value;
                OnPropertyChanged();
            }
        }

        public Utilisateur? AuthenticatedUser { get; private set; } // Pour stocker l'utilisateur connecté

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            _utilisateurRepository = new UtilisateurRepository();
            // Le paramètre de la commande sera le PasswordBox lui-même ou son SecureString
            LoginCommand = new RelayCommand(async parameter => await ExecuteLoginAsync(parameter), CanExecuteLogin);
        }

        private bool CanExecuteLogin(object? parameter)
        {
            // Le bouton de connexion est actif si le login n'est pas vide
            // La vérification du mot de passe se fera dans ExecuteLoginAsync
            return !string.IsNullOrWhiteSpace(LoginInput);
        }

        private async Task ExecuteLoginAsync(object? parameter)
        {
            ErrorMessage = string.Empty;
            IsLoginSuccessful = false;
            AuthenticatedUser = null;

            // Récupérer le PasswordBox passé en paramètre
            var passwordBox = parameter as PasswordBox;
            if (passwordBox == null)
            {
                ErrorMessage = "Erreur interne: paramètre de mot de passe manquant.";
                return;
            }

            SecureString? securePassword = passwordBox.SecurePassword; // Récupérer le SecureString
            string? password = null;

            if (securePassword != null && securePassword.Length > 0)
            {
                IntPtr bstr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(securePassword);
                try
                {
                    password = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(bstr);
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(bstr);
                    // Optionnel mais bonne pratique: Effacer le SecureString après usage si possible
                    // securePassword.Clear(); // Mais attention, passwordBox.SecurePassword le garde
                }
            }

            // Nettoyer le PasswordBox après avoir récupéré le mot de passe pour ne pas le garder en mémoire inutilement
            passwordBox.Clear();


            if (string.IsNullOrWhiteSpace(LoginInput) || string.IsNullOrEmpty(password))
            {
                ErrorMessage = "Login et mot de passe sont requis.";
                return;
            }

            try
            {
                var utilisateur = await _utilisateurRepository.GetUtilisateurByLoginAsync(LoginInput);

                if (utilisateur == null)
                {
                    ErrorMessage = "Login ou mot de passe incorrect.";
                    return;
                }

                if (!utilisateur.EstActif)
                {
                    ErrorMessage = "Ce compte utilisateur est désactivé.";
                    return;
                }

                if (PasswordHasher.VerifyPassword(password!,
                        utilisateur
                            .MotDePasseHashe)) // Utiliser password! car on a vérifié string.IsNullOrEmpty(password)
                {
                    AuthenticatedUser = utilisateur;
                    IsLoginSuccessful = true;
                }
                else
                {
                    ErrorMessage = "Login ou mot de passe incorrect.";
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine(ex.ToString()); 
                ErrorMessage = "Une erreur est survenue lors de la tentative de connexion.";
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}