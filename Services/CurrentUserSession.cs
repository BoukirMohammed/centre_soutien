// Dans Services/CurrentUserSession.cs
using centre_soutien.Models; // Pour la classe Utilisateur

namespace centre_soutien.Services
{
    public static class CurrentUserSession
    {
        public static Utilisateur? CurrentUser { get; private set; }

        public static void SetCurrentUser(Utilisateur? user)
        {
            CurrentUser = user;
        }

        public static void ClearCurrentUser()
        {
            CurrentUser = null;
        }

        public static bool IsUserLoggedIn => CurrentUser != null;
        public static bool IsAdmin => CurrentUser?.Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
        public static bool IsSecretaire => CurrentUser?.Role?.Equals("Secretaire", StringComparison.OrdinalIgnoreCase) ?? false;
        // Tu peux ajouter d'autres helpers ici, comme :
        // public static int? CurrentUserId => CurrentUser?.IDUtilisateur;
        // public static string? CurrentUserFullName => CurrentUser?.NomComplet;
    }
}