using Microsoft.EntityFrameworkCore;
using centre_soutien.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace centre_soutien.DataAccess
{
    public class ProfesseurMatiereRepository
    {
        private ApplicationDbContext CreateContext() => new ApplicationDbContext();

        // Obtenir toutes les associations ProfesseurMatiere pour un professeur donné
        public async Task<List<ProfesseurMatiere>> GetMatieresForProfesseurAsync(int professeurId)
        {
            using (var context = CreateContext())
            {
                return await context.ProfesseurMatieres
                                    .Where(pm => pm.IDProfesseur == professeurId)
                                    .Include(pm => pm.Matiere) // Charger les détails de la matière associée
                                    .ToListAsync();
            }
        }

        // Obtenir une association spécifique par IDProfesseur et IDMatiere
        public async Task<ProfesseurMatiere?> GetAssociationAsync(int professeurId, int matiereId)
        {
            using (var context = CreateContext())
            {
                return await context.ProfesseurMatieres
                                    .FirstOrDefaultAsync(pm => pm.IDProfesseur == professeurId && pm.IDMatiere == matiereId);
            }
        }

        // Ajouter une nouvelle association ou mettre à jour une existante (Upsert simplifié)
        // Cette méthode est utile pour la fenêtre de gestion où l'on coche/décoche et modifie les pourcentages.
        public async Task AddOrUpdateAssociationAsync(ProfesseurMatiere association)
        {
            if (association == null) throw new ArgumentNullException(nameof(association));
            if (association.IDProfesseur <= 0 || association.IDMatiere <= 0)
                throw new ArgumentException("IDProfesseur et IDMatiere doivent être valides.");

            using (var context = CreateContext())
            {
                var existingAssociation = await context.ProfesseurMatieres
                    .FirstOrDefaultAsync(pm => pm.IDProfesseur == association.IDProfesseur && pm.IDMatiere == association.IDMatiere);

                if (existingAssociation != null)
                {
                    // Mettre à jour le pourcentage de l'association existante
                    existingAssociation.PourcentageRemuneration = association.PourcentageRemuneration;
                    context.ProfesseurMatieres.Update(existingAssociation);
                }
                else
                {
                    // Ajouter une nouvelle association
                    // S'assurer que le pourcentage est valide (par exemple, pas négatif)
                    if (association.PourcentageRemuneration < 0)
                        throw new ArgumentOutOfRangeException(nameof(association.PourcentageRemuneration), "Le pourcentage ne peut pas être négatif.");
                    
                    context.ProfesseurMatieres.Add(association);
                }
                await context.SaveChangesAsync();
            }
        }
        
        // Mettre à jour une liste d'associations pour un professeur.
        // Cela pourrait impliquer de supprimer les anciennes non présentes dans la nouvelle liste,
        // d'ajouter les nouvelles, et de mettre à jour celles qui existent.
        // C'est une approche plus complète pour synchroniser.
        public async Task UpdateAssociationsForProfesseurAsync(int professeurId, List<ProfesseurMatiere> nouvellesAssociations)
        {
            using (var context = CreateContext())
            {
                // 1. Obtenir les associations existantes pour ce professeur
                var associationsExistantes = await context.ProfesseurMatieres
                                                        .Where(pm => pm.IDProfesseur == professeurId)
                                                        .ToListAsync();

                // 2. Identifier les associations à supprimer
                // Celles qui sont dans associationsExistantes mais pas dans nouvellesAssociations
                var associationsASupprimer = associationsExistantes
                    .Where(existante => !nouvellesAssociations.Any(nouvelle => nouvelle.IDMatiere == existante.IDMatiere))
                    .ToList();
                
                if (associationsASupprimer.Any())
                    context.ProfesseurMatieres.RemoveRange(associationsASupprimer);

                // 3. Identifier les associations à ajouter ou à mettre à jour
                foreach (var nouvelleAssociation in nouvellesAssociations)
                {
                    nouvelleAssociation.IDProfesseur = professeurId; // S'assurer que l'IDProfesseur est correct
                    if (nouvelleAssociation.PourcentageRemuneration < 0) // Valider le pourcentage
                         throw new ArgumentOutOfRangeException(nameof(nouvelleAssociation.PourcentageRemuneration), $"Le pourcentage pour la matière ID {nouvelleAssociation.IDMatiere} ne peut pas être négatif.");


                    var associationExistante = associationsExistantes
                        .FirstOrDefault(existante => existante.IDMatiere == nouvelleAssociation.IDMatiere);

                    if (associationExistante != null)
                    {
                        // Mettre à jour le pourcentage si différent
                        if (associationExistante.PourcentageRemuneration != nouvelleAssociation.PourcentageRemuneration)
                        {
                            associationExistante.PourcentageRemuneration = nouvelleAssociation.PourcentageRemuneration;
                            context.ProfesseurMatieres.Update(associationExistante);
                        }
                    }
                    else
                    {
                        // Ajouter la nouvelle association
                        context.ProfesseurMatieres.Add(nouvelleAssociation);
                    }
                }
                await context.SaveChangesAsync();
            }
        }


        // Supprimer une association spécifique
        public async Task RemoveAssociationAsync(int professeurId, int matiereId)
        {
            using (var context = CreateContext())
            {
                var association = await context.ProfesseurMatieres
                    .FirstOrDefaultAsync(pm => pm.IDProfesseur == professeurId && pm.IDMatiere == matiereId);

                if (association != null)
                {
                    // Avant de supprimer, vérifier si cette association est utilisée quelque part
                    // (par exemple, si un professeur est lié à un groupe via cette matière).
                    // Pour l'instant, on supprime directement.
                    // bool estUtilisee = await context.Groupes.AnyAsync(g => g.IDProfesseur == professeurId && g.IDMatiere == matiereId && !g.EstArchive);
                    // if (estUtilisee)
                    // {
                    //    throw new InvalidOperationException("Impossible de supprimer cette association matière/professeur car elle est utilisée dans des groupes actifs.");
                    // }

                    context.ProfesseurMatieres.Remove(association);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}