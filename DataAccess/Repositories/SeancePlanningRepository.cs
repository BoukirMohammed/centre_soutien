// Dans DataAccess/SeancePlanningRepository.cs
using Microsoft.EntityFrameworkCore;
using centre_soutien.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Globalization;

namespace centre_soutien.DataAccess
{
    public class SeancePlanningRepository
    {
        private ApplicationDbContext CreateContext() => new ApplicationDbContext();

        // Récupère tous les créneaux actifs avec leurs détails
        public async Task<List<SeancePlanning>> GetAllActiveSeancesWithDetailsAsync()
        {
            using (var context = CreateContext())
            {
                return await context.SeancesPlanning
                                    .Where(s => s.EstActif) // Filtrer par les créneaux actifs
                                    .Include(s => s.Groupe!).ThenInclude(g => g.Matiere) 
                                    .Include(s => s.Groupe!).ThenInclude(g => g.Professeur)
                                    .Include(s => s.Salle)
                                    .OrderBy(s => s.JourSemaine).ThenBy(s => s.HeureDebut) // Ordonner par jour puis heure
                                    .ToListAsync();
            }
        }
        
        // Note: GetSeancesForDateRangeAsync n'est plus directement applicable car nous gérons des créneaux récurrents.
        // L'affichage d'un planning pour une semaine donnée se fera en filtrant les créneaux actifs
        // dont le JourSemaine et la plage de validité correspondent.


        public async Task AddSeanceAsync(SeancePlanning nouveauCreneau)
        {
            if (nouveauCreneau == null) throw new ArgumentNullException(nameof(nouveauCreneau));
            ValidateSeanceTimes(nouveauCreneau.HeureDebut, nouveauCreneau.HeureFin);
            ValidateDateRange(nouveauCreneau.DateDebutValidite, nouveauCreneau.DateFinValidite);

            using (var context = CreateContext())
            {
                // Vérification de conflits
                if (await HasConflict(context, nouveauCreneau, 0)) // 0 pour un nouveau créneau (ID à exclure)
                {
                    // Le message d'erreur exact du conflit sera levé par HasConflict ou ses sous-méthodes
                    // On pourrait ici simplement relancer ou personnaliser le message.
                    // HasConflict lèvera une InvalidOperationException si un conflit est trouvé.
                }

                nouveauCreneau.EstActif = true; // S'assurer qu'il est actif par défaut
                context.SeancesPlanning.Add(nouveauCreneau);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateSeanceAsync(SeancePlanning creneauAMettreAJour)
        {
            if (creneauAMettreAJour == null) throw new ArgumentNullException(nameof(creneauAMettreAJour));
            ValidateSeanceTimes(creneauAMettreAJour.HeureDebut, creneauAMettreAJour.HeureFin);
            ValidateDateRange(creneauAMettreAJour.DateDebutValidite, creneauAMettreAJour.DateFinValidite);

            using (var context = CreateContext())
            {
                var existingCreneau = await context.SeancesPlanning.FindAsync(creneauAMettreAJour.IDSeance);
                if (existingCreneau == null) throw new KeyNotFoundException("Créneau de planification non trouvé.");
                
                if (await HasConflict(context, creneauAMettreAJour, creneauAMettreAJour.IDSeance)) // Exclure le créneau actuel
                {
                    // HasConflict lèvera une InvalidOperationException si un conflit est trouvé.
                }

                // Appliquer les modifications à l'entité existante suivie par le contexte
                context.Entry(existingCreneau).CurrentValues.SetValues(creneauAMettreAJour);
                // S'assurer que les propriétés de navigation ne sont pas écrasées si elles ne sont pas censées l'être
                // et que l'état de l'entité est bien 'Modified' si CurrentValues.SetValues ne suffit pas
                // context.Entry(existingCreneau).State = EntityState.Modified; // Peut être redondant si CurrentValues suffit

                await context.SaveChangesAsync();
            }
        }

        public async Task DeactivateSeanceAsync(int seanceId) 
        {
            using (var context = CreateContext())
            {
                var creneau = await context.SeancesPlanning.FindAsync(seanceId);
                if (creneau != null)
                {
                    creneau.EstActif = false;
                    await context.SaveChangesAsync();
                }
            }
        }
        
        public async Task ActivateSeanceAsync(int seanceId) 
        {
            using (var context = CreateContext())
            {
                var creneau = await context.SeancesPlanning.FindAsync(seanceId);
                if (creneau != null)
                {
                    creneau.EstActif = true;
                    await context.SaveChangesAsync();
                }
            }
        }
        
        public async Task DeleteSeanceAsync(int seanceId) // Suppression physique
        {
             using (var context = CreateContext())
            {
                var creneau = await context.SeancesPlanning.FindAsync(seanceId);
                if (creneau != null)
                {
                    // Tu pourrais ajouter des vérifications ici: par exemple, si des exceptions sont liées à ce créneau.
                    // Ou si des inscriptions dépendent d'une manière ou d'une autre de l'existence de ce créneau programmé.
                    context.SeancesPlanning.Remove(creneau);
                    await context.SaveChangesAsync();
                }
            }
        }

        // Dans DataAccess/SeancePlanningRepository.cs

private async Task<bool> HasConflict(ApplicationDbContext context, SeancePlanning creneauAVerifier, int idCreneauAExclure)
{
    var groupeDuCreneauAVerifier = await context.Groupes
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(g => g.IDGroupe == creneauAVerifier.IDGroupe);
    
    if (groupeDuCreneauAVerifier == null) 
        throw new InvalidOperationException("Le groupe spécifié pour le créneau n'existe pas.");
    
    int idProfesseurAVerifier = groupeDuCreneauAVerifier.IDProfesseur;

    // 1. Récupérer les créneaux qui pourraient potentiellement entrer en conflit (même jour, actif, pas le créneau exclu)
    var creneauxCandidats = await context.SeancesPlanning
        .Where(s => s.IDSeance != idCreneauAExclure &&
                    s.EstActif && 
                    s.JourSemaine == creneauAVerifier.JourSemaine)
        .Include(s => s.Groupe) // Nécessaire pour vérifier le professeur du groupe
        .ToListAsync(); // Ramener en mémoire

    // 2. Filtrer ces candidats pour ceux dont les plages de dates de validité se chevauchent
    var creneauxPotentiellementConflictuels = creneauxCandidats
        .Where(s => DatesRangesOverlap(s.DateDebutValidite, s.DateFinValidite,
                                       creneauAVerifier.DateDebutValidite, creneauAVerifier.DateFinValidite))
        .ToList();

    // 3. Pour chaque créneau potentiellement conflictuel, vérifier si les heures se chevauchent
    foreach (var creneauExistant in creneauxPotentiellementConflictuels)
    {
        if (TimeOverlaps(creneauExistant.HeureDebut, creneauExistant.HeureFin, creneauAVerifier.HeureDebut, creneauAVerifier.HeureFin))
        {
            // Conflit d'heure détecté, maintenant vérifier si c'est pour la même salle, prof, ou groupe
            if (creneauExistant.IDSalle == creneauAVerifier.IDSalle)
            {
                // Pour un message plus clair, on pourrait charger le nom de la salle, etc.
                // Mais pour la logique, c'est suffisant.
                throw new InvalidOperationException($"Conflit de salle: La salle est déjà occupée par le groupe '{creneauExistant.Groupe?.NomDescriptifGroupe}' (ID: {creneauExistant.Groupe?.IDGroupe}) ce jour-là à ce créneau, pendant une période de validité qui se chevauche.");
            }
            
            if (creneauExistant.Groupe != null && creneauExistant.Groupe.IDProfesseur == idProfesseurAVerifier)
            {
                throw new InvalidOperationException($"Conflit de professeur: Le professeur est déjà assigné au groupe '{creneauExistant.Groupe?.NomDescriptifGroupe}' (ID: {creneauExistant.Groupe?.IDGroupe}) ce jour-là à ce créneau, pendant une période de validité qui se chevauche.");
            }
            
            if (creneauExistant.IDGroupe == creneauAVerifier.IDGroupe)
            {
                 throw new InvalidOperationException($"Conflit de groupe: Ce groupe ('{creneauExistant.Groupe?.NomDescriptifGroupe}') a déjà une autre séance planifiée ce jour-là à ce créneau, pendant une période de validité qui se chevauche.");
            }
        }
    }
    return false; // Si on arrive ici, aucun conflit n'a été trouvé et aucune exception levée
}
        private bool DatesRangesOverlap(string start1Str, string? end1Str, string start2Str, string? end2Str)
        {
            DateTime start1 = DateTime.ParseExact(start1Str, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime start2 = DateTime.ParseExact(start2Str, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime end1 = string.IsNullOrEmpty(end1Str) ? DateTime.MaxValue.Date : DateTime.ParseExact(end1Str, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime end2 = string.IsNullOrEmpty(end2Str) ? DateTime.MaxValue.Date : DateTime.ParseExact(end2Str, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return start1 <= end2 && start2 <= end1;
        }

        private bool TimeOverlaps(string heureDebut1Str, string heureFin1Str, string heureDebut2Str, string heureFin2Str)
        {
            try
            {
                TimeSpan start1 = TimeSpan.ParseExact(heureDebut1Str, "hh\\:mm", CultureInfo.InvariantCulture);
                TimeSpan end1 = TimeSpan.ParseExact(heureFin1Str, "hh\\:mm", CultureInfo.InvariantCulture);
                TimeSpan start2 = TimeSpan.ParseExact(heureDebut2Str, "hh\\:mm", CultureInfo.InvariantCulture);
                TimeSpan end2 = TimeSpan.ParseExact(heureFin2Str, "hh\\:mm", CultureInfo.InvariantCulture);
                return start1 < end2 && start2 < end1;
            }
            catch (FormatException) { throw new FormatException("Format d'heure invalide pour la comparaison. Utilisez HH:MM."); }
        }
        
        public void ValidateSeanceTimes(string heureDebutStr, string heureFinStr)
        {
             try
            {
                var heureDebut = TimeSpan.ParseExact(heureDebutStr, "hh\\:mm", CultureInfo.InvariantCulture);
                var heureFin = TimeSpan.ParseExact(heureFinStr, "hh\\:mm", CultureInfo.InvariantCulture);
                if (heureDebut >= heureFin)
                    throw new ArgumentException("L'heure de début doit être antérieure à l'heure de fin.");
            }
            catch (FormatException) { throw new FormatException("Format d'heure invalide. Utilisez HH:MM."); }
        }
        
        public void ValidateDateRange(string dateDebutStr, string? dateFinStr)
        {
            try
            {
                DateTime dateDebut = DateTime.ParseExact(dateDebutStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(dateFinStr))
                {
                    DateTime dateFin = DateTime.ParseExact(dateFinStr, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    if (dateDebut > dateFin)
                    {
                        throw new ArgumentException("La date de début de validité doit être antérieure ou égale à la date de fin.");
                    }
                }
            }
            catch (FormatException) { throw new FormatException("Format de date invalide. Utilisez YYYY-MM-DD."); }
        }
    }
}