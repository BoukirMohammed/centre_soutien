using Microsoft.EntityFrameworkCore;
using centre_soutien.Models;
using centre_soutien.DataAccess.Repositories;
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
        /// Obtient l'état des paiements par mois pour un étudiant et une année donnée
        /// </summary>
        public async Task<List<StatutPaiementMensuel>> GetStatutPaiementParMoisAsync(int etudiantId, int annee)
        {
            using (var context = CreateContext())
            {
                // Récupérer les inscriptions actives de l'étudiant
                var inscriptionsActives = await context.Inscriptions
                    .Where(i => i.IDEtudiant == etudiantId && i.EstActif)
                    .Include(i => i.DetailsPaiements)
                    .ThenInclude(dp => dp.Paiement)
                    .ToListAsync();

                var statutsPaiements = new List<StatutPaiementMensuel>();

                // Pour chaque mois de l'année
                for (int mois = 1; mois <= 12; mois++)
                {
                    var anneeMoisConcerne = $"{annee}-{mois:D2}";

                    // Calculer le montant total dû pour ce mois
                    double montantTotalDu = inscriptionsActives.Sum(i => i.PrixConvenuMensuel);

                    // Calculer le montant payé pour ce mois
                    double montantPaye = 0;
                    var detailsPourCeMois = inscriptionsActives
                        .SelectMany(i => i.DetailsPaiements)
                        .Where(dp => dp.AnneeMoisConcerne == anneeMoisConcerne)
                        .ToList();

                    montantPaye = detailsPourCeMois.Sum(dp => dp.MontantPayePourEcheance);

                    // Créer le statut pour ce mois
                    var statut = new StatutPaiementMensuel
                    {
                        EtudiantId = etudiantId,
                        Annee = annee,
                        Mois = mois,
                        NomMois = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mois),
                        MontantDu = montantTotalDu,
                        MontantPaye = montantPaye
                        // MontantRestant est calculé automatiquement, pas besoin de l'assigner
                    };

                    statutsPaiements.Add(statut);
                }

                return statutsPaiements;
            }
        }

        /// <summary>
        /// Récupère tous les paiements d'un étudiant
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
        /// Ajoute un nouveau paiement
        /// </summary>
        public async Task AddPaiementAsync(Paiement nouveauPaiement)
        {
            if (nouveauPaiement == null) throw new ArgumentNullException(nameof(nouveauPaiement));

            using (var context = CreateContext())
            {
                // Valider que le montant total correspond à la somme des détails
                var montantDetailsTotal = nouveauPaiement.DetailsPaiements?.Sum(dp => dp.MontantPayePourEcheance) ?? 0;
                if (Math.Abs(nouveauPaiement.MontantTotalRecuTransaction - montantDetailsTotal) > 0.01)
                {
                    throw new InvalidOperationException(
                        "Le montant total du paiement ne correspond pas à la somme des détails.");
                }

                nouveauPaiement.DatePaiement = DateTime.Now.ToString("yyyy-MM-dd");
                context.Paiements.Add(nouveauPaiement);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Récupère les détails de paiement pour un mois spécifique
        /// </summary>
        public async Task<List<DetailPaiement>> GetDetailsPaiementPourMoisAsync(int etudiantId, string anneeMois)
        {
            using (var context = CreateContext())
            {
                return await context.DetailsPaiements
                    .Where(dp => dp.Inscription.IDEtudiant == etudiantId &&
                                 dp.AnneeMoisConcerne == anneeMois)
                    .Include(dp => dp.Paiement)
                    .Include(dp => dp.Inscription)
                    .ThenInclude(i => i.Groupe)
                    .ThenInclude(g => g.Matiere)
                    .OrderByDescending(dp => dp.Paiement.DatePaiement)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Calcule le solde actuel d'un étudiant (négatif = en retard, positif = avance)
        /// </summary>
        public async Task<double> CalculerSoldeEtudiantAsync(int etudiantId)
        {
            using (var context = CreateContext())
            {
                var inscriptionsActives = await context.Inscriptions
                    .Where(i => i.IDEtudiant == etudiantId && i.EstActif)
                    .Include(i => i.DetailsPaiements)
                    .ToListAsync();

                if (!inscriptionsActives.Any())
                    return 0;

                // Calculer le montant total dû depuis le début des inscriptions jusqu'à maintenant
                var dateActuelle = DateTime.Now;
                var premiereDateInscription = inscriptionsActives
                    .Min(i => DateTime.Parse(i.DateInscription));

                double montantTotalDu = 0;
                double montantTotalPaye = 0;

                // Pour chaque inscription active
                foreach (var inscription in inscriptionsActives)
                {
                    var dateInscription = DateTime.Parse(inscription.DateInscription);

                    // Calculer le nombre de mois écoulés depuis l'inscription
                    var moisEcoules = ((dateActuelle.Year - dateInscription.Year) * 12) +
                        dateActuelle.Month - dateInscription.Month;

                    // Si nous sommes avant le jour d'échéance du mois actuel, ne pas compter le mois actuel
                    if (dateActuelle.Day < inscription.JourEcheanceMensuelle)
                        moisEcoules--;

                    // Le montant dû pour cette inscription
                    montantTotalDu += Math.Max(0, moisEcoules + 1) * inscription.PrixConvenuMensuel;

                    // Le montant payé pour cette inscription
                    montantTotalPaye += inscription.DetailsPaiements?.Sum(dp => dp.MontantPayePourEcheance) ?? 0;
                }

                return montantTotalPaye - montantTotalDu; // Positif = avance, négatif = retard
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
                    var moisInscription = DateTime.ParseExact(inscription.DateInscription, "yyyy-MM-dd",
                        CultureInfo.InvariantCulture);
                    var moisCourant = new DateTime(moisInscription.Year, moisInscription.Month, 1);
                    var moisReference = new DateTime(dateReference.Year, dateReference.Month, 1);

                    while (moisCourant <= moisReference)
                    {
                        var moisString = moisCourant.ToString("yyyy-MM");
                        var dateEcheance = new DateTime(moisCourant.Year, moisCourant.Month,
                            inscription.JourEcheanceMensuelle);

                        // Si le jour d'échéance n'existe pas dans le mois (ex: 31 février), prendre le dernier jour du mois
                        if (dateEcheance.Month != moisCourant.Month)
                        {
                            dateEcheance = new DateTime(moisCourant.Year, moisCourant.Month,
                                DateTime.DaysInMonth(moisCourant.Year, moisCourant.Month));
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
                            EstEnRetard = dateEcheance < dateReference &&
                                          montantDejaPaye < inscription.PrixConvenuMensuel,
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
                    DernierPaiement = paiements.Any()
                        ? DateTime.ParseExact(paiements.OrderByDescending(p => p.DatePaiement).First().DatePaiement,
                            "yyyy-MM-dd", CultureInfo.InvariantCulture)
                        : (DateTime?)null,
                    MontantEnRetard = echeances.Where(e => e.EstEnRetard).Sum(e => e.MontantRestant),
                    NombreMoisEnRetard = echeances.Count(e => e.EstEnRetard && e.MontantRestant > 0)
                };
            }
        }

        /// <summary>
        /// Obtient l'état des paiements par mois pour un étudiant, une inscription spécifique et une année donnée
        /// </summary>
        public async Task<List<StatutPaiementMensuel>> GetStatutPaiementParMoisEtMatiereAsync(int etudiantId,
            int inscriptionId, int annee)
        {
            using (var context = CreateContext())
            {
                // Récupérer l'inscription spécifique
                var inscription = await context.Inscriptions
                    .Where(i => i.IDInscription == inscriptionId &&
                                i.IDEtudiant == etudiantId &&
                                i.EstActif)
                    .Include(i => i.DetailsPaiements)
                    .ThenInclude(dp => dp.Paiement)
                    .Include(i => i.Groupe)
                    .ThenInclude(g => g.Matiere)
                    .FirstOrDefaultAsync();

                var statutsPaiements = new List<StatutPaiementMensuel>();

                // Si l'inscription n'existe pas, retourner une liste vide
                if (inscription == null)
                    return statutsPaiements;

                // Pour chaque mois de l'année
                for (int mois = 1; mois <= 12; mois++)
                {
                    var anneeMoisConcerne = $"{annee}-{mois:D2}";

                    // Le montant dû pour cette inscription ce mois-ci
                    double montantDu = inscription.PrixConvenuMensuel;

                    // Calculer le montant payé pour ce mois et cette inscription
                    var detailsPourCeMois = inscription.DetailsPaiements
                        .Where(dp => dp.AnneeMoisConcerne == anneeMoisConcerne)
                        .ToList();

                    double montantPaye = detailsPourCeMois.Sum(dp => dp.MontantPayePourEcheance);

                    // Créer le statut pour ce mois (les couleurs et icônes sont calculées automatiquement)
                    var statut = new StatutPaiementMensuel
                    {
                        EtudiantId = etudiantId,
                        InscriptionId = inscriptionId,
                        Annee = annee,
                        Mois = mois,
                        NomMois = GetNomMoisAbrege(mois), // Version abrégée pour l'affichage
                        MontantDu = montantDu,
                        MontantPaye = montantPaye
                        // Les propriétés Icone, CouleurFond, et Tooltip sont calculées automatiquement
                    };

                    statutsPaiements.Add(statut);
                }

                return statutsPaiements;
            }
        }

        /// <summary>
        /// Obtient le nom abrégé du mois
        /// </summary>
        private string GetNomMoisAbrege(int mois)
        {
            return mois switch
            {
                1 => "Jan",
                2 => "Fév",
                3 => "Mar",
                4 => "Avr",
                5 => "Mai",
                6 => "Jun",
                7 => "Jul",
                8 => "Aoû",
                9 => "Sep",
                10 => "Oct",
                11 => "Nov",
                12 => "Déc",
                _ => "???"
            };
        }

        /// <summary>
        /// Obtient le nom complet du mois
        /// </summary>
        private string GetNomMoisComplet(int mois)
        {
            return CultureInfo.GetCultureInfo("fr-FR").DateTimeFormat.GetMonthName(mois);
        }


        /// <summary>
        /// Supprime un paiement et ses détails
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
                var datePaiement =
                    DateTime.ParseExact(paiement.DatePaiement, "yyyy-MM-dd", CultureInfo.InvariantCulture);
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
}