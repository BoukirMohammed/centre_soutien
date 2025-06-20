using Microsoft.EntityFrameworkCore;
using centre_soutien.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace centre_soutien.DataAccess
{
    public class EtudiantRepository
    {
        private ApplicationDbContext CreateContext() => new ApplicationDbContext();

        public async Task<List<Etudiant>> GetAllEtudiantsAsync()
        {
            using (var context = CreateContext())
            {
                return await context.Etudiants
                                    .Where(e => !e.EstArchive)
                                    .OrderBy(e => e.Nom).ThenBy(e => e.Prenom)
                                    .ToListAsync();
            }
        }

        public async Task AddEtudiantAsync(Etudiant nouvelEtudiant)
        {
            if (nouvelEtudiant == null) throw new ArgumentNullException(nameof(nouvelEtudiant));

            using (var context = CreateContext())
            {
                // Optionnel : Vérifier l'unicité du téléphone si renseigné
                if (!string.IsNullOrEmpty(nouvelEtudiant.Telephone))
                {
                    bool telephoneExisteDeja = await context.Etudiants
                                                            .AnyAsync(e => e.Telephone == nouvelEtudiant.Telephone && !e.EstArchive);
                    if (telephoneExisteDeja)
                    {
                        throw new InvalidOperationException($"Un autre étudiant actif avec le numéro de téléphone '{nouvelEtudiant.Telephone}' existe déjà.");
                    }
                }

                // ✅ NOUVEAU : Générer automatiquement un code unique si pas déjà défini
                if (string.IsNullOrEmpty(nouvelEtudiant.Code))
                {
                    nouvelEtudiant.Code = await GenerateUniqueCodeAsync(context);
                }
                else
                {
                    // Vérifier l'unicité du code fourni
                    bool codeExisteDeja = await context.Etudiants.AnyAsync(e => e.Code == nouvelEtudiant.Code);
                    if (codeExisteDeja)
                    {
                        throw new InvalidOperationException($"Le code '{nouvelEtudiant.Code}' est déjà utilisé par un autre étudiant.");
                    }
                }

                nouvelEtudiant.DateInscriptionSysteme = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                context.Etudiants.Add(nouvelEtudiant);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateEtudiantAsync(Etudiant etudiantAMettreAJour)
        {
            if (etudiantAMettreAJour == null) throw new ArgumentNullException(nameof(etudiantAMettreAJour));

            using (var context = CreateContext())
            {
                // Optionnel : Vérifier l'unicité du téléphone si modifié
                if (!string.IsNullOrEmpty(etudiantAMettreAJour.Telephone))
                {
                    bool telephoneExisteDeja = await context.Etudiants
                                                            .AnyAsync(e => e.Telephone == etudiantAMettreAJour.Telephone && 
                                                                           !e.EstArchive &&
                                                                           e.IDEtudiant != etudiantAMettreAJour.IDEtudiant);
                    if (telephoneExisteDeja)
                    {
                        throw new InvalidOperationException($"Un autre étudiant actif avec le numéro de téléphone '{etudiantAMettreAJour.Telephone}' existe déjà.");
                    }
                }

                // ✅ NOUVEAU : Vérifier l'unicité du code si modifié
                if (!string.IsNullOrEmpty(etudiantAMettreAJour.Code))
                {
                    bool codeExisteDeja = await context.Etudiants
                                                        .AnyAsync(e => e.Code == etudiantAMettreAJour.Code && 
                                                                       e.IDEtudiant != etudiantAMettreAJour.IDEtudiant);
                    if (codeExisteDeja)
                    {
                        throw new InvalidOperationException($"Le code '{etudiantAMettreAJour.Code}' est déjà utilisé par un autre étudiant.");
                    }
                }

                context.Etudiants.Update(etudiantAMettreAJour);
                await context.SaveChangesAsync();
            }
        }

        public async Task ArchiveEtudiantAsync(int etudiantId)
        {
            using (var context = CreateContext())
            {
                var etudiant = await context.Etudiants.FindAsync(etudiantId);
                if (etudiant != null)
                {
                    // VÉRIFICATION : Ne pas archiver si l'étudiant a des inscriptions actives
                    bool aInscriptionsActives = await context.Inscriptions
                                                              .AnyAsync(i => i.IDEtudiant == etudiantId && i.EstActif);
                    if (aInscriptionsActives)
                    {
                        throw new InvalidOperationException($"Impossible d'archiver l'étudiant '{etudiant.Prenom} {etudiant.Nom}'. Il/Elle a encore des inscriptions actives.");
                    }

                    etudiant.EstArchive = true;
                    await context.SaveChangesAsync();
                }
            }
        }

        // ✅ NOUVELLE MÉTHODE : Rechercher un étudiant par code
        public async Task<Etudiant?> GetEtudiantByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            using (var context = CreateContext())
            {
                return await context.Etudiants
                                    .FirstOrDefaultAsync(e => e.Code == code && !e.EstArchive);
            }
        }

        // ✅ NOUVELLE MÉTHODE : Générer un code unique
        private async Task<string> GenerateUniqueCodeAsync(ApplicationDbContext context)
        {
            const int maxAttempts = 100; // Éviter une boucle infinie
            var random = new Random();
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // Format: ETU + année + 4 chiffres aléatoires
                string code = $"ETU{DateTime.Now.Year}{random.Next(1000, 9999)}";
                
                bool codeExists = await context.Etudiants.AnyAsync(e => e.Code == code);
                if (!codeExists)
                {
                    return code;
                }
            }
            
            // Si après 100 tentatives on n'arrive pas à générer un code unique,
            // utiliser l'horodatage pour garantir l'unicité
            string timestampCode = $"ETU{DateTime.Now:yyyyMMddHHmmss}";
            return timestampCode;
        }

        // ✅ NOUVELLE MÉTHODE : Régénérer un code pour un étudiant existant
        public async Task RegenerateCodeAsync(int etudiantId)
        {
            using (var context = CreateContext())
            {
                var etudiant = await context.Etudiants.FindAsync(etudiantId);
                if (etudiant != null)
                {
                    etudiant.Code = await GenerateUniqueCodeAsync(context);
                    await context.SaveChangesAsync();
                }
                else
                {
                    throw new KeyNotFoundException($"Étudiant avec ID {etudiantId} non trouvé.");
                }
            }
        }

        // ✅ NOUVELLE MÉTHODE : Vérifier si un code existe déjà
        public async Task<bool> CodeExistsAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;

            using (var context = CreateContext())
            {
                return await context.Etudiants.AnyAsync(e => e.Code == code);
            }
        }

        // ✅ NOUVELLE MÉTHODE : Recherche par code ou nom/prénom
        public async Task<List<Etudiant>> SearchEtudiantsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) 
                return await GetAllEtudiantsAsync();

            using (var context = CreateContext())
            {
                return await context.Etudiants
                    .Where(e => !e.EstArchive && 
                                (e.Code.Contains(searchTerm) ||
                                 e.Nom.Contains(searchTerm) ||
                                 e.Prenom.Contains(searchTerm) ||
                                 (e.Nom + " " + e.Prenom).Contains(searchTerm) ||
                                 (e.Prenom + " " + e.Nom).Contains(searchTerm)))
                    .OrderBy(e => e.Nom).ThenBy(e => e.Prenom)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Suppression simple - échoue s'il y a des données liées
        /// </summary>
        public async Task DeleteEtudiantAsync(int etudiantId)
        {
            using (var context = CreateContext())
            {
                var etudiant = await context.Etudiants.FindAsync(etudiantId);
                if (etudiant == null)
                {
                    throw new InvalidOperationException("L'étudiant à supprimer n'existe pas.");
                }

                try
                {
                    // Vérifications métier avant suppression
                    var referencesInfo = await GetEtudiantReferencesAsync(context, etudiantId);
                    
                    if (referencesInfo.HasAnyReferences)
                    {
                        throw new InvalidOperationException(
                            $"Impossible de supprimer l'étudiant '{etudiant.Prenom} {etudiant.Nom}'.\n" +
                            $"Raisons :\n{referencesInfo.GetErrorMessage()}\n\n" +
                            "💡 Solution : Utilisez l'archivage à la place, ou la suppression forcée si nécessaire.");
                    }

                    // Si aucune référence, suppression possible
                    context.Etudiants.Remove(etudiant);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message?.Contains("FOREIGN KEY constraint") == true)
                {
                    // Gestion des contraintes de clés étrangères non détectées
                    throw new InvalidOperationException(
                        $"Impossible de supprimer l'étudiant '{etudiant.Prenom} {etudiant.Nom}' " +
                        "car il est référencé dans d'autres tables. " +
                        "Utilisez l'archivage ou la suppression forcée.");
                }
            }
        }

        /// <summary>
        /// Suppression forcée - supprime l'étudiant ET toutes ses données liées
        /// </summary>
        public async Task ForceDeleteEtudiantAsync(int etudiantId)
        {
            using var context = CreateContext();
            using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                var etudiant = await context.Etudiants.FindAsync(etudiantId);
                if (etudiant == null)
                {
                    throw new InvalidOperationException("L'étudiant à supprimer n'existe pas.");
                }

                // 1. Supprimer les paiements liés
                var paiements = await context.Paiements
                    .Where(p => p.IDEtudiant == etudiantId)
                    .ToListAsync();
                
                if (paiements.Any())
                {
                    context.Paiements.RemoveRange(paiements);
                    await context.SaveChangesAsync(); // Sauvegarder étape par étape
                }

                // 2. Supprimer les inscriptions liées
                var inscriptions = await context.Inscriptions
                    .Where(i => i.IDEtudiant == etudiantId)
                    .ToListAsync();
                
                if (inscriptions.Any())
                {
                    context.Inscriptions.RemoveRange(inscriptions);
                    await context.SaveChangesAsync();
                }

                // 3. Supprimer d'autres références si elles existent
                // Ajoutez ici d'autres tables qui référencent l'étudiant
                /*
                var autresReferences = await context.AutreTable
                    .Where(a => a.IDEtudiant == etudiantId)
                    .ToListAsync();
                if (autresReferences.Any())
                {
                    context.AutreTable.RemoveRange(autresReferences);
                    await context.SaveChangesAsync();
                }
                */

                // 4. Enfin, supprimer l'étudiant
                context.Etudiants.Remove(etudiant);
                await context.SaveChangesAsync();

                // 5. Confirmer la transaction
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Méthode privée pour analyser les références d'un étudiant
        /// </summary>
        private async Task<EtudiantReferencesInfo> GetEtudiantReferencesAsync(ApplicationDbContext context, int etudiantId)
        {
            var info = new EtudiantReferencesInfo();

            // Vérifier les inscriptions
            info.InscriptionsCount = await context.Inscriptions
                .CountAsync(i => i.IDEtudiant == etudiantId);

            info.InscriptionsActivesCount = await context.Inscriptions
                .CountAsync(i => i.IDEtudiant == etudiantId && i.EstActif);

            // Vérifier les paiements
            info.PaiementsCount = await context.Paiements
                .CountAsync(p => p.IDEtudiant == etudiantId);

            // Vérifier les paiements récents (moins de 6 mois)
            var dateLimit = DateTime.Now.AddMonths(-6);
            info.PaiementsRecentsCount = await context.Paiements
                .CountAsync(p => p.IDEtudiant == etudiantId && 
                               DateTime.Parse(p.DatePaiement) >= dateLimit);

            return info;
        }

        /// <summary>
        /// Obtenir un résumé des données liées à un étudiant (pour l'interface utilisateur)
        /// </summary>
        public async Task<string> GetEtudiantDataSummaryAsync(int etudiantId)
        {
            using var context = CreateContext();
            var info = await GetEtudiantReferencesAsync(context, etudiantId);
            
            if (!info.HasAnyReferences)
            {
                return "✅ Aucune donnée associée - Suppression simple possible";
            }

            var summary = "📊 Données associées détectées :\n";
            
            if (info.InscriptionsCount > 0)
            {
                summary += $"• {info.InscriptionsCount} inscription(s)";
                if (info.InscriptionsActivesCount > 0)
                    summary += $" (dont {info.InscriptionsActivesCount} active(s))";
                summary += "\n";
            }

            if (info.PaiementsCount > 0)
            {
                summary += $"• {info.PaiementsCount} paiement(s)";
                if (info.PaiementsRecentsCount > 0)
                    summary += $" (dont {info.PaiementsRecentsCount} récent(s))";
                summary += "\n";
            }

            summary += "\n💡 Recommandation : Utilisez l'archivage plutôt que la suppression.";
            
            return summary;
        }
    }

    /// <summary>
    /// Classe helper pour analyser les références d'un étudiant
    /// </summary>
    public class EtudiantReferencesInfo
    {
        public int InscriptionsCount { get; set; }
        public int InscriptionsActivesCount { get; set; }
        public int PaiementsCount { get; set; }
        public int PaiementsRecentsCount { get; set; }

        public bool HasAnyReferences => InscriptionsCount > 0 || PaiementsCount > 0;

        public string GetErrorMessage()
        {
            var errors = new List<string>();

            if (InscriptionsActivesCount > 0)
            {
                errors.Add($"• {InscriptionsActivesCount} inscription(s) active(s)");
            }
            else if (InscriptionsCount > 0)
            {
                errors.Add($"• {InscriptionsCount} inscription(s) dans l'historique");
            }

            if (PaiementsRecentsCount > 0)
            {
                errors.Add($"• {PaiementsRecentsCount} paiement(s) récent(s)");
            }
            else if (PaiementsCount > 0)
            {
                errors.Add($"• {PaiementsCount} paiement(s) dans l'historique");
            }

            return string.Join("\n", errors);
        }
    }
}