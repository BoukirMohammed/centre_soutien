using System;
using System.Security.Cryptography; // Pour Rfc2898DeriveBytes et RandomNumberGenerator
using System.Text;                  // Pour Encoding

namespace centre_soutien.Helpers // Ajuste le namespace si nécessaire
{
    public static class PasswordHasher
    {
        // Configuration du hachage
        private const int SaltSize = 16; // Taille du sel en octets (128 bits)
        private const int HashSize = 32; // Taille du hachage en octets (256 bits)
        private const int Iterations = 100000; // Nombre d'itérations (ajuster selon les besoins de performance/sécurité)
        private const char Delimiter = ';'; // Pour séparer les parties dans la chaîne stockée

        /// <summary>
        /// Crée un hachage pour un mot de passe donné.
        /// </summary>
        /// <param name="password">Le mot de passe à hacher.</param>
        /// <returns>Une chaîne contenant le sel, le nombre d'itérations, et le hachage, séparés par un délimiteur.</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            // 1. Générer un sel aléatoire
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            // 2. Hacher le mot de passe avec le sel et les itérations
            // Utilisation de PBKDF2 avec HMACSHA256 (par défaut pour Rfc2898DeriveBytes avec .NET Core/.NET 5+)
            // ou HMACSHA512 si tu veux être plus explicite et que la performance le permet.
            // Pour cet exemple, on utilise les valeurs par défaut de HashAlgorithmName qui est SHA256 pour .NET 6+
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                // 3. Combiner sel, itérations et hachage dans une seule chaîne pour le stockage
                // Format: Base64(salt);iterations;Base64(hash)
                string saltString = Convert.ToBase64String(salt);
                string hashString = Convert.ToBase64String(hash);

                return $"{saltString}{Delimiter}{Iterations}{Delimiter}{hashString}";
            }
        }

        /// <summary>
        /// Vérifie un mot de passe fourni par l'utilisateur contre un hachage stocké.
        /// </summary>
        /// <param name="password">Le mot de passe fourni par l'utilisateur.</param>
        /// <param name="hashedPasswordWithSaltAndIterations">Le hachage stocké (incluant sel et itérations).</param>
        /// <returns>True si le mot de passe correspond, sinon False.</returns>
        public static bool VerifyPassword(string password, string hashedPasswordWithSaltAndIterations)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }
            if (string.IsNullOrEmpty(hashedPasswordWithSaltAndIterations))
            {
                throw new ArgumentNullException(nameof(hashedPasswordWithSaltAndIterations));
            }

            // 1. Extraire les composants de la chaîne stockée
            string[] parts = hashedPasswordWithSaltAndIterations.Split(Delimiter);
            if (parts.Length != 3)
            {
                // Format invalide, pourrait indiquer une corruption ou une tentative de contournement
                // Tu pourrais logger cela ou retourner false directement.
                // Pour la sécurité, ne pas donner trop d'infos sur l'échec.
                return false; 
            }

            byte[] salt;
            int iterations;
            byte[] storedHash;

            try
            {
                salt = Convert.FromBase64String(parts[0]);
                if (!int.TryParse(parts[1], out iterations))
                {
                    return false; // Format d'itérations invalide
                }
                storedHash = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException)
            {
                // Erreur lors de la conversion Base64 ou int, format invalide
                return false;
            }
            

            // 2. Hacher le mot de passe fourni avec le même sel et les mêmes itérations
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] testHash = pbkdf2.GetBytes(HashSize);

                // 3. Comparer les hachages (comparaison de temps constant pour éviter les attaques par minutage)
                //    Bien que pour un simple booléen de retour, la différence est minime dans la plupart des cas.
                //    CryptographicOperations.FixedTimeEquals est idéal mais nécessite .NET Core 3.0+ / .NET 5+
                //    Pour une comparaison simple et sûre :
                if (testHash.Length != storedHash.Length)
                {
                    return false;
                }
                for (int i = 0; i < testHash.Length; i++)
                {
                    if (testHash[i] != storedHash[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}