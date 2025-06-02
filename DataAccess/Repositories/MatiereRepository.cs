using Microsoft.EntityFrameworkCore;
using centre_soutien.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System; // Pour ArgumentNullException et InvalidOperationException

namespace centre_soutien.DataAccess
{
    public class MatiereRepository
    {
        private ApplicationDbContext CreateContext()
        {
            return new ApplicationDbContext();
        }

        // Obtenir toutes les matières non archivées
        public async Task<List<Matiere>> GetAllMatieresAsync()
        {
            using (var context = CreateContext())
            {
                return await context.Matieres
                                    .Where(m => !m.EstArchivee)
                                    .OrderBy(m => m.NomMatiere)
                                    .ToListAsync();
            }
        }

        // Obtenir une matière par son ID
        public async Task<Matiere> GetMatiereByIdAsync(int matiereId)
        {
            using (var context = CreateContext())
            {
                return await context.Matieres.FindAsync(matiereId);
            }
        }

        // Ajouter une nouvelle matière
        public async Task AddMatiereAsync(Matiere nouvelleMatiere)
        {
            if (nouvelleMatiere == null)
                throw new ArgumentNullException(nameof(nouvelleMatiere));

            using (var context = CreateContext())
            {
                bool nomExisteDeja = await context.Matieres
                                                .AnyAsync(m => m.NomMatiere == nouvelleMatiere.NomMatiere && !m.EstArchivee);
                if (nomExisteDeja)
                {
                    throw new InvalidOperationException($"Une matière active avec le nom '{nouvelleMatiere.NomMatiere}' existe déjà.");
                }

                context.Matieres.Add(nouvelleMatiere);
                await context.SaveChangesAsync();
            }
        }

        // Mettre à jour une matière existante
        public async Task UpdateMatiereAsync(Matiere matiereAMettreAJour)
        {
            if (matiereAMettreAJour == null)
                throw new ArgumentNullException(nameof(matiereAMettreAJour));

            using (var context = CreateContext())
            {
                bool nomExisteDeja = await context.Matieres
                                                .AnyAsync(m => m.NomMatiere == matiereAMettreAJour.NomMatiere &&
                                                               !m.EstArchivee &&
                                                               m.IDMatiere != matiereAMettreAJour.IDMatiere);
                if (nomExisteDeja)
                {
                    throw new InvalidOperationException($"Une autre matière active avec le nom '{matiereAMettreAJour.NomMatiere}' existe déjà.");
                }
                
                // On s'assure que l'entité est suivie avant de la mettre à jour
                var existingMatiere = await context.Matieres.FindAsync(matiereAMettreAJour.IDMatiere);
                if (existingMatiere != null)
                {
                    // Copier les valeurs de matiereAMettreAJour vers existingMatiere
                    // ou utiliser context.Entry(existingMatiere).CurrentValues.SetValues(matiereAMettreAJour);
                    existingMatiere.NomMatiere = matiereAMettreAJour.NomMatiere;
                    existingMatiere.PrixStandardMensuel = matiereAMettreAJour.PrixStandardMensuel;
                    existingMatiere.Description = matiereAMettreAJour.Description;
                    // L'état d'archivage est géré par une méthode séparée.
                    // existingMatiere.EstArchivee = matiereAMettreAJour.EstArchivee;

                    // context.Matieres.Update(existingMatiere); // Pas toujours nécessaire si l'entité est suivie
                    await context.SaveChangesAsync();
                }
                else
                {
                     throw new KeyNotFoundException($"Matière avec ID {matiereAMettreAJour.IDMatiere} non trouvée.");
                }
            }
        }

        // Archiver une matière
       public async Task ArchiveMatiereAsync(int matiereId)
{
    using (var context = CreateContext()) // Assure-toi que CreateContext() retourne une nouvelle instance de ApplicationDbContext
    {
        var matiere = await context.Matieres
                                   // Optionnel: Inclure les groupes pour afficher leurs noms dans le message d'erreur si tu le souhaites
                                   // .Include(m => m.Groupes) 
                                   .FirstOrDefaultAsync(m => m.IDMatiere == matiereId);

        if (matiere == null)
        {
            throw new KeyNotFoundException($"Matière avec ID {matiereId} non trouvée.");
        }

        if (matiere.EstArchivee) // Si déjà archivée, ne rien faire ou informer
        {
            // Optionnel: Tu peux retourner ou lever une exception différente
            // throw new InvalidOperationException($"La matière '{matiere.NomMatiere}' est déjà archivée.");
            return; // Ou simplement ne rien faire
        }

        // --- VÉRIFICATION SI LA MATIÈRE EST UTILISÉE DANS DES GROUPES ACTIFS ---
        // Un groupe est "actif" s'il n'est pas lui-même archivé.
        bool estUtiliseeDansGroupesActifs = await context.Groupes
                                                    .AnyAsync(g => g.IDMatiere == matiereId && g.EstArchive == false);

        if (estUtiliseeDansGroupesActifs)
        {
            // Tu pourrais récupérer les noms des groupes pour un message plus précis, mais AnyAsync est plus performant pour juste vérifier l'existence.
            // string nomsGroupes = string.Join(", ", context.Groupes
            //                                               .Where(g => g.IDMatiere == matiereId && !g.EstArchive)
            //                                               .Select(g => g.NomDescriptifGroupe)
            //                                               .ToList()); // Attention, ToList() est synchrone ici, pour un vrai message, il faudrait être async.

            throw new InvalidOperationException($"Impossible d'archiver la matière '{matiere.NomMatiere}'. Elle est encore assignée à un ou plusieurs groupes actifs. Veuillez d'abord archiver ou modifier ces groupes.");
        }

        // --- AUTRES VÉRIFICATIONS OPTIONNELLES (Exemple pour ProfesseurMatieres) ---
        // Si tu décides que l'association à un professeur bloque l'archivage de la matière :
        /*
        bool estAssocieeAProfesseurs = await context.ProfesseurMatieres
                                                  .AnyAsync(pm => pm.IDMatiere == matiereId); 
                                                  // Tu pourrais ajouter une condition sur l'état d'archivage du professeur lié
                                                  // en utilisant Include pour charger Professeur puis vérifier pm.Professeur.EstArchive == false

        if (estAssocieeAProfesseurs)
        {
            throw new InvalidOperationException($"Impossible d'archiver la matière '{matiere.NomMatiere}'. Elle est encore associée à un ou plusieurs professeurs. Veuillez d'abord gérer ces associations.");
        }
        */

        // Si toutes les vérifications sont passées, on peut archiver la matière
        matiere.EstArchivee = true;
        // context.Matieres.Update(matiere); // EF Core suit les changements sur 'matiere' car elle a été chargée depuis le context.
        await context.SaveChangesAsync();
    }
}
    }
}