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
                nouvelEtudiant.DateInscriptionSysteme = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // Assurer la date système
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
    }
}