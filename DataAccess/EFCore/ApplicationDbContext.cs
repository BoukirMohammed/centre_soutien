using Microsoft.EntityFrameworkCore;
using centre_soutien.Models; // Assure-toi que ce using pointe vers tes modèles
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace centre_soutien.DataAccess // Ajuste le namespace si nécessaire
{
    public class ApplicationDbContext : DbContext
    {
        // DbSet pour chaque entité/table
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Etudiant> Etudiants { get; set; }
        public DbSet<Professeur> Professeurs { get; set; }
        public DbSet<Matiere> Matieres { get; set; }
        public DbSet<ProfesseurMatiere> ProfesseurMatieres { get; set; }
        public DbSet<Salle> Salles { get; set; }
        public DbSet<Groupe> Groupes { get; set; }
        public DbSet<Inscription> Inscriptions { get; set; }
        public DbSet<Paiement> Paiements { get; set; }
        public DbSet<DetailPaiement> DetailsPaiements { get; set; }
        public DbSet<SeancePlanning> SeancesPlanning { get; set; }

        // Configuration de la connexion
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                const string DatabaseFileName = "CentreSoutienEF.db";
                string executablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!; // L'opérateur ! supprime le warning CS8600 ici
                string dbPath = Path.Combine(executablePath, DatabaseFileName);

                optionsBuilder.UseSqlite($"Data Source={dbPath}")
                    // --- AJOUTER CES LIGNES POUR LA JOURNALISATION ---
                    .LogTo(Console.WriteLine, LogLevel.Information) // Affiche les logs dans la console de sortie
                    .EnableSensitiveDataLogging(); // Affiche les valeurs des paramètres SQL (utile pour le débogage, mais attention en production)
                // --- FIN DES LIGNES POUR LA JOURNALISATION ---
            }
        }

        // Configuration des modèles (Fluent API)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configuration des Entités et Relations ---

            // Utilisateurs
            modelBuilder.Entity<Utilisateur>(entity =>
            {
                entity.HasKey(e => e.IDUtilisateur);
                entity.Property(e => e.Login).IsRequired().HasMaxLength(100); // Exemple de longueur max
                entity.HasIndex(e => e.Login).IsUnique();
                entity.Property(e => e.Role).IsRequired();
                // Les booléens sont gérés nativement par EF Core pour SQLite (INTEGER 0 ou 1)
            });

            // Etudiants
            modelBuilder.Entity<Etudiant>(entity =>
            {
                entity.HasKey(e => e.IDEtudiant);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Prenom).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Telephone).IsUnique().HasFilter("[Telephone] IS NOT NULL"); // Unique si non null
                // Pour les dates stockées en TEXT, EF Core les gère bien comme string.
                // Si tu utilisais DateTime, tu pourrais spécifier HasConversion ou HasColumnType.
            });

            // Professeurs
            modelBuilder.Entity<Professeur>(entity =>
            {
                entity.HasKey(e => e.IDProfesseur);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Prenom).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Telephone).IsUnique().HasFilter("[Telephone] IS NOT NULL");
            });

            // Matieres
            modelBuilder.Entity<Matiere>(entity =>
            {
                entity.HasKey(e => e.IDMatiere);
                entity.Property(e => e.NomMatiere).IsRequired().HasMaxLength(150);
                entity.HasIndex(e => e.NomMatiere).IsUnique();
                entity.Property(e => e.PrixStandardMensuel).HasColumnType("REAL"); // Explicite pour SQLite
            });

            // ProfesseurMatieres (Table d'association)
            modelBuilder.Entity<ProfesseurMatiere>(entity =>
            {
                entity.HasKey(e => e.IDProfesseurMatiere); // Clé primaire simple auto-incrémentée
                entity.HasIndex(pm => new { pm.IDProfesseur, pm.IDMatiere }).IsUnique(); // Assure l'unicité de la paire

                entity.HasOne(pm => pm.Professeur)
                    .WithMany(p => p.ProfesseurMatieres) // Nécessite ICollection<ProfesseurMatiere> ProfesseurMatieres dans Professeur.cs
                    .HasForeignKey(pm => pm.IDProfesseur)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(pm => pm.Matiere)
                    .WithMany(m => m.ProfesseurMatieres) // Nécessite ICollection<ProfesseurMatiere> ProfesseurMatieres dans Matiere.cs
                    .HasForeignKey(pm => pm.IDMatiere)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Salles
            modelBuilder.Entity<Salle>(entity =>
            {
                entity.HasKey(e => e.IDSalle);
                entity.Property(e => e.NomOuNumeroSalle).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.NomOuNumeroSalle).IsUnique();
            });

            // Groupes
            modelBuilder.Entity<Groupe>(entity =>
            {
                entity.HasKey(e => e.IDGroupe);
                entity.Property(e => e.NomDescriptifGroupe).IsRequired().HasMaxLength(200);
                entity.HasIndex(g => new { g.NomDescriptifGroupe, g.IDMatiere, g.IDProfesseur })
                    .IsUnique()
                    .HasDatabaseName("IX_Groupes_Nom_Matiere_Prof_Unique"); 
                entity.HasOne(g => g.Matiere)
                    .WithMany(m => m.Groupes) // Nécessite ICollection<Groupe> Groupes dans Matiere.cs
                    .HasForeignKey(g => g.IDMatiere)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.Professeur)
                    .WithMany(p => p.Groupes) // Nécessite ICollection<Groupe> Groupes dans Professeur.cs
                    .HasForeignKey(g => g.IDProfesseur)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Inscriptions
           // Dans OnModelCreating de ApplicationDbContext.cs
modelBuilder.Entity<Inscription>(entity =>
{
    entity.HasKey(e => e.IDInscription);
    entity.Property(e => e.DateInscription).IsRequired();
    entity.Property(e => e.PrixConvenuMensuel).HasColumnType("REAL").IsRequired();
    entity.Property(e => e.JourEcheanceMensuelle).IsRequired();

    // Relation avec Etudiant
    entity.HasOne(i => i.Etudiant)
        .WithMany(et => et.Inscriptions) // Dans Etudiant.cs : public virtual ICollection<Inscription> Inscriptions ...
        .HasForeignKey(i => i.IDEtudiant)
        .OnDelete(DeleteBehavior.Cascade); // Si un étudiant est supprimé, ses inscriptions le sont aussi (à discuter)
                                            // Ou Restrict si on veut empêcher la suppression d'un étudiant avec inscriptions

    // Relation avec Groupe
    entity.HasOne(i => i.Groupe)
        .WithMany(g => g.Inscriptions) // Dans Groupe.cs : public virtual ICollection<Inscription> Inscriptions ...
        .HasForeignKey(i => i.IDGroupe)
        .OnDelete(DeleteBehavior.Restrict); // Empêche la suppression d'un groupe s'il a des inscriptions

    // Contrainte d'unicité pour qu'un étudiant ne soit pas inscrit activement plusieurs fois au même groupe
    // Cette contrainte peut être délicate si vous permettez la réinscription après désinscription.
    // Une approche simple : un étudiant ne peut avoir qu'UNE SEULE inscription active à un groupe donné.
    // Pour SQLite, HasFilter n'est pas supporté nativement pour les index uniques de cette manière.
    // On peut mettre un index unique sur (IDEtudiant, IDGroupe) et gérer la logique "EstActif" applicativement
    // ou lors de la création pour s'assurer qu'il n'y a pas d'autre inscription active pour cette paire.
    // Ou, si l'on veut permettre plusieurs inscriptions (une active, les autres archivées),
    // alors l'unicité devrait inclure un discriminant ou être gérée différemment.
    // Pour l'instant, mettons un index unique sur la paire et la logique de vérification d'activité sera dans le Repository.
    entity.HasIndex(i => new { i.IDEtudiant, i.IDGroupe })
          .IsUnique()
          .HasDatabaseName("IX_Inscriptions_Etudiant_Groupe_Unique"); 
          // Ce qui signifie qu'un étudiant ne peut être inscrit qu'une seule fois (active ou non) à un groupe.
          // Si tu veux permettre une inscription, puis désinscription, puis réinscription au même groupe,
          // cet index unique doit être retiré ou modifié pour inclure un statut ou une date de validité.
          // Pour l'instant, on part sur le principe qu'une ligne IDEtudiant-IDGroupe est unique.
});

            // Paiements
            modelBuilder.Entity<Paiement>(entity =>
            {
                entity.HasKey(e => e.IDPaiement);
                entity.Property(e => e.MontantTotalRecuTransaction).HasColumnType("REAL");

                entity.HasOne(p => p.Etudiant)
                    .WithMany(et => et.Paiements) // Nécessite ICollection<Paiement> Paiements dans Etudiant.cs
                    .HasForeignKey(p => p.IDEtudiant)
                    .OnDelete(DeleteBehavior.Restrict); // On ne supprime pas un étudiant s'il a des paiements

                entity.HasOne(p => p.UtilisateurEnregistrement)
                    .WithMany(u => u.PaiementsEnregistres) // Nécessite ICollection<Paiement> PaiementsEnregistres dans Utilisateur.cs
                    .HasForeignKey(p => p.IDUtilisateurEnregistrement)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // DetailsPaiements
            modelBuilder.Entity<DetailPaiement>(entity =>
            {
                entity.HasKey(e => e.IDDetailPaiement);
                entity.Property(e => e.MontantPayePourEcheance).HasColumnType("REAL");

                entity.HasOne(dp => dp.Paiement)
                    .WithMany(p => p.DetailsPaiements) // Nécessite ICollection<DetailPaiement> DetailsPaiements dans Paiement.cs
                    .HasForeignKey(dp => dp.IDPaiement)
                    .OnDelete(DeleteBehavior.Cascade); // Si on supprime un paiement, ses détails sont supprimés

                entity.HasOne(dp => dp.Inscription)
                    .WithMany(i => i.DetailsPaiements) // Nécessite ICollection<DetailPaiement> DetailsPaiements dans Inscription.cs
                    .HasForeignKey(dp => dp.IDInscription)
                    .OnDelete(DeleteBehavior.Restrict); // On ne supprime pas une inscription si elle a des détails de paiement
            });

            // SeancesPlanning
// Dans OnModelCreating de ApplicationDbContext.cs
// Dans DataAccess/ApplicationDbContext.cs, méthode OnModelCreating

            // Dans DataAccess/ApplicationDbContext.cs, méthode OnModelCreating

            modelBuilder.Entity<SeancePlanning>(entity =>
            {
                entity.HasKey(e => e.IDSeance);

                entity.Property(e => e.JourSemaine).IsRequired();
                entity.Property(e => e.HeureDebut).IsRequired().HasMaxLength(5); // "HH:MM"
                entity.Property(e => e.HeureFin).IsRequired().HasMaxLength(5);   // "HH:MM"
                entity.Property(e => e.DateDebutValidite).IsRequired();
                // DateFinValidite est nullable

                // Relation avec Groupe
                entity.HasOne(sp => sp.Groupe)
                    .WithMany(g => g.SeancesPlanning)
                    .HasForeignKey(sp => sp.IDGroupe)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relation avec Salle
                entity.HasOne(sp => sp.Salle)
                    .WithMany(s => s.SeancesPlanning)
                    .HasForeignKey(sp => sp.IDSalle)
                    .OnDelete(DeleteBehavior.Restrict);

                // Contraintes d'unicité pour le même jour et heure de début
                // La logique de chevauchement des plages horaires et des plages de dates de validité
                // sera principalement gérée dans le Repository.
                entity.HasIndex(sp => new { sp.IDSalle, sp.JourSemaine, sp.HeureDebut })
                    .IsUnique() // Attention: Cet index seul n'empêche pas tous les conflits de chevauchement d'heure
                    .HasDatabaseName("IX_Seances_Salle_Jour_HeureDebut_Unique");

                entity.HasIndex(sp => new { sp.IDGroupe, sp.JourSemaine, sp.HeureDebut })
                    .IsUnique() // De même ici
                    .HasDatabaseName("IX_Seances_Groupe_Jour_HeureDebut_Unique");
            });
            // --- Fin de la Configuration ---
        }
    }
}