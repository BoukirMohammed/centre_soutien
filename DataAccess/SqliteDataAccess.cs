using Microsoft.Data.Sqlite; // Assure-toi d'utiliser le bon namespace pour ton driver SQLite
using System.IO;             // Pour Path.Combine
using System.Reflection;     // Pour Assembly.GetExecutingAssembly().Location

namespace centre_soutien.DataAccess // Ajuste le namespace si nécessaire
{
    public static class SqliteDataAccess
    {
        // Nom du fichier de la base de données
        private const string DatabaseFileName = "CentreSoutien.db";

        // Méthode pour obtenir la chaîne de connexion
        public static string GetConnectionString()
        {
            // Obtient le chemin du répertoire de l'exécutable de l'application
            // C'est important car le fichier .db sera copié dans le répertoire de sortie
            string executablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string dbPath = Path.Combine(executablePath, DatabaseFileName);

            return $"Data Source={dbPath}";
        }

        // Méthode pour obtenir une nouvelle connexion à la base de données
        // Utile pour Dapper car il attend une IDbConnection
        public static SqliteConnection GetConnection()
        {
            return new SqliteConnection(GetConnectionString());
        }
    }
}