using Microsoft.EntityFrameworkCore;
using centre_soutien.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace centre_soutien.DataAccess
{
    public class ProfesseurRepository
    {
        private ApplicationDbContext CreateContext() => new ApplicationDbContext();

        public async Task<List<Professeur>> GetAllProfesseursAsync()
        {
            using (var context = CreateContext())
            {
                return await context.Professeurs
                                    .Where(p => !p.EstArchive)
                                    .OrderBy(p => p.Nom).ThenBy(p => p.Prenom)
                                    .ToListAsync();
            }
        }

        public async Task AddProfesseurAsync(Professeur nouveauProfesseur)
        {
            if (nouveauProfesseur == null) throw new ArgumentNullException(nameof(nouveauProfesseur));

            using (var context = CreateContext())
            {
                // Optionnel : vérifier si un professeur avec le même nom/prénom ou téléphone existe déjà
                context.Professeurs.Add(nouveauProfesseur);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateProfesseurAsync(Professeur professeurAMettreAJour)
        {
            if (professeurAMettreAJour == null) throw new ArgumentNullException(nameof(professeurAMettreAJour));

            using (var context = CreateContext())
            {
                context.Professeurs.Update(professeurAMettreAJour);
                await context.SaveChangesAsync();
            }
        }

        public async Task ArchiveProfesseurAsync(int professeurId)
        {
            using (var context = CreateContext())
            {
                var professeur = await context.Professeurs.FindAsync(professeurId);
                if (professeur != null)
                {
                    // VÉRIFICATION : Ne pas archiver si le professeur est assigné à des groupes actifs
                    bool estAssigneAGroupeActif = await context.Groupes
                                                                .AnyAsync(g => g.IDProfesseur == professeurId && !g.EstArchive);
                    if (estAssigneAGroupeActif)
                    {
                        throw new InvalidOperationException($"Impossible d'archiver le professeur '{professeur.Prenom} {professeur.Nom}'. Il/Elle est encore assigné(e) à des groupes actifs.");
                    }

                    professeur.EstArchive = true;
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}