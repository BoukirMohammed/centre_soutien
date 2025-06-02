namespace centre_soutien.DataAccess.Repositories;

using Microsoft.EntityFrameworkCore; // Pour ToListAsync, Include, etc.
using centre_soutien.Models;      // Pour accéder à la classe Salle
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;     // Pour les opérations asynchrones

    public class SalleRepository
    {
        // Méthode pour obtenir une instance du DbContext.
        // Pour des applications plus grandes, on utiliserait l'injection de dépendances.
        // Pour commencer simplement, on peut le créer directement.
        private ApplicationDbContext CreateContext()
        {
            return new ApplicationDbContext();
        }

        // Obtenir toutes les salles non archivées
        public async Task<List<Salle>> GetAllSallesAsync()
        {
            using (var context = CreateContext())
            {
                return await context.Salles
                                    .Where(s => !s.EstArchivee) // Ne prend que les salles non archivées
                                    .OrderBy(s => s.NomOuNumeroSalle)
                                    .ToListAsync();
            }
        }

        // Obtenir une salle par son ID
        public async Task<Salle> GetSalleByIdAsync(int salleId)
        {
            using (var context = CreateContext())
            {
                return await context.Salles.FindAsync(salleId);
                // FindAsync fonctionne bien pour les clés primaires.
                // Si tu veux inclure des données liées (ex: SeancesPlanning), tu ferais :
                // return await context.Salles
                //                     .Include(s => s.SeancesPlanning)
                //                     .FirstOrDefaultAsync(s => s.IDSalle == salleId);
            }
        }

        // Ajouter une nouvelle salle
        // Dans SalleRepository.cs - AddSalleAsync
        public async Task AddSalleAsync(Salle nouvelleSalle)
        {
            if (nouvelleSalle == null)
                throw new ArgumentNullException(nameof(nouvelleSalle));

            using (var context = CreateContext())
            {
                string nomNormalise = nouvelleSalle.NomOuNumeroSalle.Trim(); // Ou ToLowerInvariant()

                bool nomExisteDeja = await context.Salles
                    .AnyAsync(s => s.NomOuNumeroSalle.Trim() == nomNormalise && 
                                   !s.EstArchivee);
                if (nomExisteDeja)
                {
                    throw new InvalidOperationException($"Une salle active avec le nom '{nouvelleSalle.NomOuNumeroSalle}' existe déjà.");
                }

                // Si tu veux stocker le nom normalisé :
                // nouvelleSalle.NomOuNumeroSalle = nomNormalise;

                context.Salles.Add(nouvelleSalle);
                try
                {
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException is Microsoft.Data.Sqlite.SqliteException sqliteEx && sqliteEx.SqliteErrorCode == 19)
                    {
                        throw new InvalidOperationException($"Le nom de salle '{nouvelleSalle.NomOuNumeroSalle}' existe déjà ou une autre contrainte d'unicité a été violée.", ex);
                    }
                    throw;
                }
            }
        }
        // Mettre à jour une salle existante
// Dans DataAccess/SalleRepository.cs

public async Task UpdateSalleAsync(Salle salleAMettreAJour)
{
    if (salleAMettreAJour == null)
        throw new ArgumentNullException(nameof(salleAMettreAJour));

    using (var context = CreateContext())
    {
        // 1. Vérifier si le nouveau nom (si différent) est déjà utilisé par une AUTRE salle active
        // On normalise les noms pour la comparaison (ex: enlever les espaces de début/fin et mettre en minuscule)
        string nomNormaliseAMettreAJour = salleAMettreAJour.NomOuNumeroSalle.Trim(); // Peut-être .ToLowerInvariant() aussi

        bool nomExisteDejaPourUneAutreSalle = await context.Salles
            .AnyAsync(s => s.NomOuNumeroSalle.Trim() == nomNormaliseAMettreAJour && // Comparer les noms normalisés
                           !s.EstArchivee &&
                           s.IDSalle != salleAMettreAJour.IDSalle); // Exclure la salle actuelle

        if (nomExisteDejaPourUneAutreSalle)
        {
            throw new InvalidOperationException($"Une autre salle active avec le nom '{salleAMettreAJour.NomOuNumeroSalle}' existe déjà.");
        }

        // 2. Récupérer l'entité existante de la base de données
        var salleExistante = await context.Salles.FindAsync(salleAMettreAJour.IDSalle);

        if (salleExistante == null)
        {
            throw new KeyNotFoundException($"Salle avec ID {salleAMettreAJour.IDSalle} non trouvée pour la mise à jour.");
        }

        // 3. Mettre à jour les propriétés de l'entité existante avec les valeurs de salleAMettreAJour
        // Cela assure qu'EF Core suit correctement les changements sur une entité qu'il connaît.
        salleExistante.NomOuNumeroSalle = salleAMettreAJour.NomOuNumeroSalle; // Ou nomNormaliseAMettreAJour si tu veux stocker la version trimée
        salleExistante.Capacite = salleAMettreAJour.Capacite;
        salleExistante.Description = salleAMettreAJour.Description;
        // Ne pas modifier EstArchivee ici, cela devrait être géré par une méthode ArchiveSalleAsync dédiée.
        // salleExistante.EstArchivee = salleAMettreAJour.EstArchivee; 

        // context.Salles.Update(salleExistante); // Pas toujours nécessaire si l'entité est suivie
        // OU utiliser context.Entry(salleExistante).CurrentValues.SetValues(salleAMettreAJour);
        // mais la mise à jour manuelle des propriétés comme ci-dessus est souvent plus claire.

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) // Intercepter spécifiquement DbUpdateException
        {
            // Tu peux inspecter ex.InnerException pour voir si c'est une SqliteException avec ErrorCode 19 (UNIQUE constraint)
            // et potentiellement renvoyer un message plus convivial si la vérification en amont n'a pas tout attrapé
            // (par exemple, un cas de concurrence rare).
            if (ex.InnerException is Microsoft.Data.Sqlite.SqliteException sqliteEx && sqliteEx.SqliteErrorCode == 19)
            {
                 throw new InvalidOperationException($"Le nom de salle '{salleAMettreAJour.NomOuNumeroSalle}' existe déjà ou une autre contrainte d'unicité a été violée.", ex);
            }
            throw; // Relancer l'exception si ce n'est pas une violation d'unicité connue
        }
    }
}

        // Archiver une salle (suppression logique)
        public async Task ArchiveSalleAsync(int salleId)
        {
            using (var context = CreateContext())
            {
                var salle = await context.Salles.FindAsync(salleId);
                if (salle != null)
                {
                    // Avant d'archiver, vérifier si la salle est utilisée dans SeancesPlanning non terminées/annulées
                    // bool estUtilisee = await context.SeancesPlanning
                    //                              .AnyAsync(sp => sp.IDSalle == salleId && sp.StatutSeance != "Effectuée" && sp.StatutSeance != "AnnuléeCentre" && sp.StatutSeance != "AnnuléeProf");
                    // if (estUtilisee)
                    // {
                    //    throw new InvalidOperationException("Impossible d'archiver la salle car elle est utilisée dans des séances planifiées actives.");
                    // }

                    salle.EstArchivee = true;
                    // context.Salles.Update(salle); // Pas toujours nécessaire si le suivi des modifications est activé
                    await context.SaveChangesAsync();
                }
            }
        }

        // Si tu avais besoin d'une suppression physique (à utiliser avec précaution)
        // public async Task DeleteSallePhysicallyAsync(int salleId)
        // {
        //     using (var context = CreateContext())
        //     {
        //         var salle = await context.Salles.FindAsync(salleId);
        //         if (salle != null)
        //         {
        //             // Vérifications importantes avant suppression physique (dépendances)
        //             context.Salles.Remove(salle);
        //             await context.SaveChangesAsync();
        //         }
        //     }
        // }
    }