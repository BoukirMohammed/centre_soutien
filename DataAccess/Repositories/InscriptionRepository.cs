using Microsoft.EntityFrameworkCore;
using centre_soutien.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace centre_soutien.DataAccess
{
    public class InscriptionRepository
    {
        private ApplicationDbContext CreateContext() => new ApplicationDbContext();

        // Obtenir toutes les inscriptions (avec détails étudiant et groupe)
        public async Task<List<Inscription>> GetAllInscriptionsWithDetailsAsync()
        {
            using (var context = CreateContext())
            {
                return await context.Inscriptions
                                    .Include(i => i.Etudiant)
                                    .Include(i => i.Groupe)
                                        .ThenInclude(g => g.Matiere) // Pour afficher le nom de la matière du groupe
                                    .Include(i => i.Groupe)
                                        .ThenInclude(g => g.Professeur) // Pour afficher le nom du prof du groupe
                                    .OrderByDescending(i => i.DateInscription)
                                    .ToListAsync();
            }
        }
        // Dans DataAccess/InscriptionRepository.cs

// ... (autres méthodes) ...

        public async Task<List<Inscription>> GetActiveInscriptionsForGroupeAsync(int groupeId)
        {
            using (var context = CreateContext())
            {
                return await context.Inscriptions
                    .Where(i => i.IDGroupe == groupeId && i.EstActif)
                    .Include(i => i.Etudiant) // Charger les détails de l'étudiant
                    .OrderBy(i => i.Etudiant!.Nom) // Utiliser ! si Etudiant est censé être toujours là
                    .ThenBy(i => i.Etudiant!.Prenom)
                    .ToListAsync();
            }
        }
        
        // Obtenir les inscriptions actives pour un étudiant spécifique
        public async Task<List<Inscription>> GetActiveInscriptionsForEtudiantAsync(int etudiantId)
        {
            using (var context = CreateContext())
            {
                return await context.Inscriptions
                                    .Where(i => i.IDEtudiant == etudiantId && i.EstActif)
                                    .Include(i => i.Groupe.Matiere) // Charger Matiere via Groupe
                                    .Include(i => i.Groupe.Professeur) // Charger Professeur via Groupe
                                    .OrderBy(i => i.Groupe.Matiere.NomMatiere)
                                    .ToListAsync();
            }
        }


        public async Task AddInscriptionAsync(Inscription nouvelleInscription)
        {
            if (nouvelleInscription == null) throw new ArgumentNullException(nameof(nouvelleInscription));

            using (var context = CreateContext())
            {
                // Vérifier si l'étudiant est déjà inscrit ACTIVEment à ce groupe
                bool dejaInscritActif = await context.Inscriptions
                    .AnyAsync(i => i.IDEtudiant == nouvelleInscription.IDEtudiant &&
                                   i.IDGroupe == nouvelleInscription.IDGroupe &&
                                   i.EstActif); // On vérifie seulement les inscriptions actives

                if (dejaInscritActif)
                {
                    // Récupérer les noms pour un message plus clair
                    var etudiant = await context.Etudiants.FindAsync(nouvelleInscription.IDEtudiant);
                    var groupe = await context.Groupes.FindAsync(nouvelleInscription.IDGroupe);
                    string nomEtudiant = etudiant != null ? $"{etudiant.Prenom} {etudiant.Nom}" : "Cet étudiant";
                    string nomGroupe = groupe != null ? groupe.NomDescriptifGroupe : "ce groupe";

                    throw new InvalidOperationException($"{nomEtudiant} est déjà inscrit(e) activement à {nomGroupe}.");
                }
                
                // Assurer que la date d'inscription et le statut actif sont corrects
                nouvelleInscription.DateInscription = DateTime.Now.ToString("yyyy-MM-dd");
                nouvelleInscription.EstActif = true;
                nouvelleInscription.DateDesinscription = null; // S'assurer qu'il n'y a pas de date de désinscription

                context.Inscriptions.Add(nouvelleInscription);
                await context.SaveChangesAsync();
            }
        }

        // Désinscrire un étudiant (marquer l'inscription comme inactive)
        public async Task DesinscrireAsync(int inscriptionId)
        {
            using (var context = CreateContext())
            {
                var inscription = await context.Inscriptions.FindAsync(inscriptionId);
                if (inscription != null)
                {
                    if (!inscription.EstActif)
                    {
                        // Optionnel: lever une exception ou juste retourner si déjà inactif
                        throw new InvalidOperationException("Cette inscription est déjà inactive.");
                    }

                    // Vérifier si des paiements sont en cours ou des choses qui empêcheraient la désinscription
                    // Pour l'instant, on la marque simplement inactive.
                    
                    inscription.EstActif = false;
                    inscription.DateDesinscription = DateTime.Now.ToString("yyyy-MM-dd");
                    await context.SaveChangesAsync();
                }
                else
                {
                    throw new KeyNotFoundException($"Inscription avec ID {inscriptionId} non trouvée.");
                }
            }
        }
        
        // Mettre à jour les détails d'une inscription (par exemple, prix convenu)
        // ATTENTION: Cette méthode est un exemple. La mise à jour d'une inscription existante
        // doit être faite avec précaution, surtout si des paiements y sont liés.
        // Modifier le prix ou le jour d'échéance peut avoir des implications.
        public async Task UpdateInscriptionDetailsAsync(Inscription inscriptionAModifier)
        {
            if (inscriptionAModifier == null) throw new ArgumentNullException(nameof(inscriptionAModifier));

            using (var context = CreateContext())
            {
                var existingInscription = await context.Inscriptions.FindAsync(inscriptionAModifier.IDInscription);
                if (existingInscription != null)
                {
                    // Mettre à jour seulement les champs modifiables
                    existingInscription.PrixConvenuMensuel = inscriptionAModifier.PrixConvenuMensuel;
                    existingInscription.JourEcheanceMensuelle = inscriptionAModifier.JourEcheanceMensuelle;
                    // Ne pas modifier IDEtudiant, IDGroupe, DateInscription, EstActif ici.
                    // EstActif est géré par DesinscrireAsync.

                    await context.SaveChangesAsync();
                }
                else
                {
                    throw new KeyNotFoundException($"Inscription avec ID {inscriptionAModifier.IDInscription} non trouvée.");
                }
            }
        }
    }
}