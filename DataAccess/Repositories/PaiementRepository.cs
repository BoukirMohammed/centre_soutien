using Microsoft.EntityFrameworkCore;
using centre_soutien.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Globalization;

namespace centre_soutien.DataAccess
{
    public class PaiementRepository
    {
        private ApplicationDbContext CreateContext() => new ApplicationDbContext();

        /// <summary>
        /// Récupère tous les paiements d'un étudiant avec les détails
        /// </summary>
        public async Task<List<Paiement>> GetPaiementsForEtudiantAsync(int etudiantId)
        {
            using (var context = CreateContext())
            {
                return await context.Paiements
                    .Where(p => p.IDEtudiant == etudiantId)
                    .Include(p => p.DetailsPaiements)
                        .ThenInclude(dp => dp.Inscription)
                            .ThenInclude(i => i.Groupe)
                                .ThenInclude(g => g.Matiere)
                    .Include(p => p.UtilisateurEnregistrement)
                    .OrderByDescending(p => p.DatePaiement)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Calcule les échéances dues pour un étudiant à une date donnée
        /// </summary>
        public async Task<List<EcheanceInfo>> GetEcheancesAPayer(int etudiantId, DateTime dateReference)
        {
            using (var context = CreateContext())
            {
                var inscriptionsActives = await context.Inscriptions
                    .Where(i => i.IDEtudiant == etudiantId && i.EstActif)
                    .Include(i => i.Groupe)
                        .ThenInclude(g => g.Matiere)
                    .ToListAsync();

                var echeances = new List<EcheanceInfo>();

                foreach (var inscription in inscriptionsActives)
                {
                    // Calculer les mois dus jusqu'à la date de référence
                    var moisInscription = DateTime.ParseExact(inscription.DateInscription, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    var moisCourant = new DateTime(moisInscription.Year, moisInscription.Month, 1);
                    var moisReference = new DateTime(dateReference.Year, dateReference.Month, 1);

                    while (moisCourant <= moisReference)
                    {
                        var moisString = moisCourant.ToString("yyyy-MM");
                        var dateEcheance = new DateTime(moisCourant.Year, moisCourant.Month, inscription.JourEcheanceMensuelle);
                        
                        // Si le jour d'échéance n'existe pas dans le mois (ex: 31 février), prendre le dernier jour du mois
                        if (dateEcheance.Month != moisCourant.Month)
                        {
                            dateEcheance = new DateTime(moisCourant.Year, moisCourant.Month, DateTime.DaysInMonth(moisCourant.Year, moisCourant.Month));
                        }

                        // Vérifier si ce mois est déjà payé
                        var montantDejaPaye = await context.DetailsPaiements
                            .Where(dp => dp.IDInscription == inscription.IDInscription && 
                                        dp.AnneeMoisConcerne == moisString)
                            .SumAsync(dp => dp.MontantPayePourEcheance);

                        var echeance = new EcheanceInfo
                        {
                            Inscription = inscription,
                            MoisConcerne = moisString,
                            DateEcheance = dateEcheance,
                            MontantDu = inscription.PrixConvenuMensuel,
                            MontantDejaPaye = montantDejaPaye,
                            MontantRestant = inscription.PrixConvenuMensuel - montantDejaPaye,
                            EstEnRetard = dateEcheance < dateReference && montantDejaPaye < inscription.PrixConvenuMensuel,
                            EstPayeCompletement = montantDejaPaye >= inscription.PrixConvenuMensuel
                        };

                        echeances.Add(echeance);
                        moisCourant = moisCourant.AddMonths(1);
                    }
                }

                return echeances.OrderBy(e => e.DateEcheance).ToList();
            }
        }

        /// <summary>
        /// Ajoute un nouveau paiement avec ses détails
        /// </summary>
        public async Task AddPaiementAsync(Paiement paiement, List<DetailPaiement> details)
        {
            if (paiement == null) throw new ArgumentNullException(nameof(paiement));
            if (details == null || !details.Any()) throw new ArgumentException("Au moins un détail de paiement est requis.", nameof(details));

            using (var context = CreateContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        // Validation : vérifier que la somme des détails correspond au montant total
                        var sommeMontants = details.Sum(d => d.MontantPayePourEcheance);
                        if (Math.Abs(sommeMontants - paiement.MontantTotalRecuTransaction) > 0.01) // Tolérance pour les arrondis
                        {
                            throw new InvalidOperationException($"La somme des détails ({sommeMontants:C}) ne correspond pas au montant total ({paiement.MontantTotalRecuTransaction:C}).");
                        }

                        // Ajouter le paiement principal
                        context.Paiements.Add(paiement);
                        await context.SaveChangesAsync();

                        // Ajouter les détails avec l'ID du paiement
                        foreach (var detail in details)
                        {
                            detail.IDPaiement = paiement.IDPaiement;
                            context.DetailsPaiements.Add(detail);
                        }

                        await context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Récupère l'historique des paiements sur une période
        /// </summary>
        public async Task<List<Paiement>> GetPaiementsParPeriodeAsync(DateTime dateDebut, DateTime dateFin)
        {
            using (var context = CreateContext())
            {
                return await context.Paiements
                    .Where(p => DateTime.ParseExact(p.DatePaiement, "yyyy-MM-dd", CultureInfo.InvariantCulture) >= dateDebut &&
                                DateTime.ParseExact(p.DatePaiement, "yyyy-MM-dd", CultureInfo.InvariantCulture) <= dateFin)
                    .Include(p => p.Etudiant)
                    .Include(p => p.UtilisateurEnregistrement)
                    .Include(p => p.DetailsPaiements)
                        .ThenInclude(dp => dp.Inscription)
                            .ThenInclude(i => i.Groupe)
                                .ThenInclude(g => g.Matiere)
                    .OrderByDescending(p => p.DatePaiement)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Récupère les statistiques de paiement pour un étudiant
        /// </summary>
        public async Task<StatistiquesPaiementEtudiant> GetStatistiquesPaiementEtudiantAsync(int etudiantId)
        {
            using (var context = CreateContext())
            {
                var paiements = await context.Paiements
                    .Where(p => p.IDEtudiant == etudiantId)
                    .ToListAsync();

                var echeances = await GetEcheancesAPayer(etudiantId, DateTime.Now);

                return new StatistiquesPaiementEtudiant
                {
                    TotalPaye = paiements.Sum(p => p.MontantTotalRecuTransaction),
                    NombrePaiements = paiements.Count,
                    DernierPaiement = paiements.Any() ? 
                        DateTime.ParseExact(paiements.OrderByDescending(p => p.DatePaiement).First().DatePaiement, "yyyy-MM-dd", CultureInfo.InvariantCulture) : 
                        (DateTime?)null,
                    MontantEnRetard = echeances.Where(e => e.EstEnRetard).Sum(e => e.MontantRestant),
                    NombreMoisEnRetard = echeances.Count(e => e.EstEnRetard && e.MontantRestant > 0)
                };
            }
        }

        /// <summary>
        /// Supprime un paiement et ses détails (avec vérifications)
        /// </summary>
        public async Task SupprimerPaiementAsync(int paiementId)
        {
            using (var context = CreateContext())
            {
                var paiement = await context.Paiements
                    .Include(p => p.DetailsPaiements)
                    .FirstOrDefaultAsync(p => p.IDPaiement == paiementId);

                if (paiement == null)
                    throw new KeyNotFoundException($"Paiement avec ID {paiementId} non trouvé.");

                // Vérifications métier (par exemple, ne pas supprimer des paiements trop anciens)
                var datePaiement = DateTime.ParseExact(paiement.DatePaiement, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                if ((DateTime.Now - datePaiement).TotalDays > 30)
                {
                    throw new InvalidOperationException("Impossible de supprimer un paiement de plus de 30 jours.");
                }

                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        // Supprimer d'abord les détails
                        context.DetailsPaiements.RemoveRange(paiement.DetailsPaiements);
                        
                        // Puis le paiement principal
                        context.Paiements.Remove(paiement);
                        
                        await context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Classe pour représenter une échéance avec ses informations de paiement
    /// </summary>
    public class EcheanceInfo
    {
        public Inscription Inscription { get; set; }
        public string MoisConcerne { get; set; } = string.Empty; // Format "yyyy-MM"
        public DateTime DateEcheance { get; set; }
        public double MontantDu { get; set; }
        public double MontantDejaPaye { get; set; }
        public double MontantRestant { get; set; }
        public bool EstEnRetard { get; set; }
        public bool EstPayeCompletement { get; set; }
        
        // Propriétés calculées pour l'affichage
        public string StatutTexte => EstPayeCompletement ? "Payé" : EstEnRetard ? "En retard" : "À payer";
        public string MoisFormate => DateTime.ParseExact(MoisConcerne + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture).ToString("MMMM yyyy", new CultureInfo("fr-FR"));
    }

    /// <summary>
    /// Statistiques de paiement pour un étudiant
    /// </summary>
    public class StatistiquesPaiementEtudiant
    {
        public double TotalPaye { get; set; }
        public int NombrePaiements { get; set; }
        public DateTime? DernierPaiement { get; set; }
        public double MontantEnRetard { get; set; }
        public int NombreMoisEnRetard { get; set; }
        public bool EstAJour => MontantEnRetard <= 0;
    }
}