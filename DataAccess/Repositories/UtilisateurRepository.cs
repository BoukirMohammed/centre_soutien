using Microsoft.EntityFrameworkCore;
using centre_soutien.Models;    // Pour la classe Utilisateur
using centre_soutien.Helpers;  // Pour PasswordHasher (si tu l'as mis dans Helpers)
using System.Threading.Tasks;
using System;                   // Pour ArgumentNullException
using System.Linq;              // Pour FirstOrDefaultAsync
using System.Collections.Generic; // Pour List<Utilisateur> (pour les futurs CRUD)

namespace centre_soutien.DataAccess
{
    public class UtilisateurRepository
    {
        private ApplicationDbContext CreateContext() => new ApplicationDbContext();

        /// <summary>
        /// Récupère un utilisateur par son login.
        /// </summary>
        /// <param name="login">Le login de l'utilisateur à rechercher.</param>
        /// <returns>L'objet Utilisateur s'il est trouvé et actif, sinon null.</returns>
        public async Task<Utilisateur?> GetUtilisateurByLoginAsync(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                return null;
            }

            using (var context = CreateContext())
            {
                // On ne retourne que les utilisateurs actifs. La vérification du mot de passe se fera après.
                return await context.Utilisateurs
                                    .FirstOrDefaultAsync(u => u.Login == login /* && u.EstActif */);
                // Note : Laisser la vérification EstActif pour la logique de connexion dans le ViewModel
                // permet de donner un message d'erreur plus spécifique ("compte désactivé")
                // plutôt qu'un simple "login/mdp incorrect".
                // Si tu préfères que le repo ne retourne que les actifs: décommente '&& u.EstActif'.
            }
        }

        // --- Méthodes CRUD pour la gestion des utilisateurs (principalement pour l'Admin plus tard) ---

        public async Task<List<Utilisateur>> GetAllUtilisateursAsync()
        {
            using (var context = CreateContext())
            {
                return await context.Utilisateurs.OrderBy(u => u.Login).ToListAsync();
            }
        }
        
        public async Task<Utilisateur?> GetUtilisateurByIdAsync(int utilisateurId)
        {
             using (var context = CreateContext())
            {
                return await context.Utilisateurs.FindAsync(utilisateurId);
            }
        }


        /// <summary>
        /// Ajoute un nouvel utilisateur. Le mot de passe fourni sera haché.
        /// </summary>
        public async Task AddUtilisateurAsync(Utilisateur nouvelUtilisateur, string motDePasseEnClair)
        {
            if (nouvelUtilisateur == null) throw new ArgumentNullException(nameof(nouvelUtilisateur));
            if (string.IsNullOrWhiteSpace(motDePasseEnClair)) throw new ArgumentException("Le mot de passe ne peut pas être vide.", nameof(motDePasseEnClair));

            using (var context = CreateContext())
            {
                // Vérifier si le login existe déjà
                bool loginExisteDeja = await context.Utilisateurs.AnyAsync(u => u.Login == nouvelUtilisateur.Login);
                if (loginExisteDeja)
                {
                    throw new InvalidOperationException($"Le login '{nouvelUtilisateur.Login}' est déjà utilisé.");
                }

                nouvelUtilisateur.MotDePasseHashe = PasswordHasher.HashPassword(motDePasseEnClair);
                nouvelUtilisateur.EstActif = true; // Par défaut, un nouvel utilisateur est actif

                context.Utilisateurs.Add(nouvelUtilisateur);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Met à jour un utilisateur existant. Ne modifie pas le mot de passe ici.
        /// Pour changer le mot de passe, utiliser une méthode dédiée.
        /// </summary>
        public async Task UpdateUtilisateurAsync(Utilisateur utilisateurAMettreAJour)
        {
            if (utilisateurAMettreAJour == null) throw new ArgumentNullException(nameof(utilisateurAMettreAJour));

            using (var context = CreateContext())
            {
                // Vérifier si le login (si modifié) n'est pas déjà pris par un AUTRE utilisateur
                bool loginExistePourAutre = await context.Utilisateurs
                    .AnyAsync(u => u.Login == utilisateurAMettreAJour.Login && u.IDUtilisateur != utilisateurAMettreAJour.IDUtilisateur);
                if (loginExistePourAutre)
                {
                    throw new InvalidOperationException($"Le login '{utilisateurAMettreAJour.Login}' est déjà utilisé par un autre utilisateur.");
                }

                var utilisateurExistant = await context.Utilisateurs.FindAsync(utilisateurAMettreAJour.IDUtilisateur);
                if (utilisateurExistant != null)
                {
                    utilisateurExistant.Login = utilisateurAMettreAJour.Login;
                    utilisateurExistant.NomComplet = utilisateurAMettreAJour.NomComplet;
                    utilisateurExistant.Role = utilisateurAMettreAJour.Role;
                    utilisateurExistant.EstActif = utilisateurAMettreAJour.EstActif;
                    // Ne pas toucher à MotDePasseHashe ici
                    
                    // context.Utilisateurs.Update(utilisateurExistant); // Pas toujours nécessaire si l'entité est suivie
                    await context.SaveChangesAsync();
                }
                else
                {
                    throw new KeyNotFoundException($"Utilisateur avec ID {utilisateurAMettreAJour.IDUtilisateur} non trouvé.");
                }
            }
        }
        
        /// <summary>
        /// Change le mot de passe d'un utilisateur.
        /// </summary>
        public async Task ChangePasswordAsync(int utilisateurId, string nouveauMotDePasseEnClair)
        {
            if (string.IsNullOrWhiteSpace(nouveauMotDePasseEnClair)) throw new ArgumentException("Le nouveau mot de passe ne peut pas être vide.", nameof(nouveauMotDePasseEnClair));

            using (var context = CreateContext())
            {
                var utilisateur = await context.Utilisateurs.FindAsync(utilisateurId);
                if (utilisateur != null)
                {
                    utilisateur.MotDePasseHashe = PasswordHasher.HashPassword(nouveauMotDePasseEnClair);
                    await context.SaveChangesAsync();
                }
                else
                {
                     throw new KeyNotFoundException($"Utilisateur avec ID {utilisateurId} non trouvé.");
                }
            }
        }

        // Tu pourrais aussi avoir des méthodes DeactivateUserAsync et ActivateUserAsync qui changent EstActif.
    }
}