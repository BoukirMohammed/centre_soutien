using Microsoft.EntityFrameworkCore;
using centre_soutien.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace centre_soutien.DataAccess
{
    public class GroupeRepository
    {
        private ApplicationDbContext CreateContext() => new ApplicationDbContext();

        public async Task<List<Groupe>> GetAllGroupesWithDetailsAsync()
        {
            using (var context = CreateContext())
            {
                return await context.Groupes
                                    .Where(g => !g.EstArchive)
                                    .Include(g => g.Matiere)     // Charger la matière associée
                                    .Include(g => g.Professeur)  // Charger le professeur associé
                                    .OrderBy(g => g.NomDescriptifGroupe)
                                    .ToListAsync();
            }
        }
        
        public async Task<Groupe?> GetGroupeByIdAsync(int groupeId) // Peut retourner null
        {
            using (var context = CreateContext())
            {
                return await context.Groupes
                                    .Include(g => g.Matiere)
                                    .Include(g => g.Professeur)
                                    .FirstOrDefaultAsync(g => g.IDGroupe == groupeId);
            }
        }


        public async Task AddGroupeAsync(Groupe nouveauGroupe)
        {
            if (nouveauGroupe == null) throw new ArgumentNullException(nameof(nouveauGroupe));

            using (var context = CreateContext())
            {
                bool combinationExists = await context.Groupes
                    .AnyAsync(g => g.NomDescriptifGroupe == nouveauGroupe.NomDescriptifGroupe &&
                                   g.IDMatiere == nouveauGroupe.IDMatiere &&
                                   g.IDProfesseur == nouveauGroupe.IDProfesseur &&
                                   !g.EstArchive);
                if (combinationExists)
                {
                    throw new InvalidOperationException(
                        $"Un groupe actif avec le nom '{nouveauGroupe.NomDescriptifGroupe}' pour cette matière et ce professeur existe déjà.");
                }
                // --- AJOUTS MANQUANTS ---
                context.Groupes.Add(nouveauGroupe); // Ajouter l'entité au contexte
                await context.SaveChangesAsync(); 
            }
        }

        public async Task UpdateGroupeAsync(Groupe groupeAMettreAJour)
        {
            if (groupeAMettreAJour == null) throw new ArgumentNullException(nameof(groupeAMettreAJour));

            using (var context = CreateContext())
            {
                bool combinationExists = await context.Groupes
                    .AnyAsync(g => g.NomDescriptifGroupe == groupeAMettreAJour.NomDescriptifGroupe &&
                                   g.IDMatiere == groupeAMettreAJour.IDMatiere &&
                                   g.IDProfesseur == groupeAMettreAJour.IDProfesseur &&
                                   !g.EstArchive &&
                                   g.IDGroupe != groupeAMettreAJour.IDGroupe);
                if (combinationExists)
                {
                    throw new InvalidOperationException($"Un autre groupe actif avec le nom '{groupeAMettreAJour.NomDescriptifGroupe}' pour cette matière et ce professeur existe déjà.");
                }
                // --- AJOUTS MANQUANTS ---
                context.Groupes.Update(groupeAMettreAJour); // Marquer l'entité comme modifiée
                await context.SaveChangesAsync();  
            }
        }

        public async Task ArchiveGroupeAsync(int groupeId)
        {
            using (var context = CreateContext())
            {
                var groupe = await context.Groupes.FindAsync(groupeId);
                if (groupe != null)
                {
                    // VÉRIFICATION : Ne pas archiver si le groupe a des inscriptions actives
                    bool aInscriptionsActives = await context.Inscriptions
                                                              .AnyAsync(i => i.IDGroupe == groupeId && i.EstActif);
                    if (aInscriptionsActives)
                    {
                        throw new InvalidOperationException($"Impossible d'archiver le groupe '{groupe.NomDescriptifGroupe}'. Il a encore des inscriptions actives.");
                    }
                    
                    // VÉRIFICATION : Ne pas archiver si le groupe a des séances planning non terminées/annulées
                    // bool aSeancesActives = await context.SeancesPlanning
                    //                                     .AnyAsync(sp => sp.IDGroupe == groupeId && 
                    //                                                    sp.StatutSeance != "Effectuée" && 
                    //                                                    sp.StatutSeance != "AnnuléeCentre" && // Adapter les statuts
                    //                                                    sp.StatutSeance != "AnnuléeProf");
                    // if (aSeancesActives)
                    // {
                    //     throw new InvalidOperationException($"Impossible d'archiver le groupe '{groupe.NomDescriptifGroupe}'. Il a encore des séances planifiées actives ou non terminées.");
                    // }


                    groupe.EstArchive = true;
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}