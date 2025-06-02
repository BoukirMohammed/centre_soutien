using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace centre_soutien.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Etudiants",
                columns: table => new
                {
                    IDEtudiant = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DateNaissance = table.Column<string>(type: "TEXT", nullable: false),
                    Adresse = table.Column<string>(type: "TEXT", nullable: false),
                    Telephone = table.Column<string>(type: "TEXT", nullable: false),
                    Lycee = table.Column<string>(type: "TEXT", nullable: false),
                    DateInscriptionSysteme = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    EstArchive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Etudiants", x => x.IDEtudiant);
                });

            migrationBuilder.CreateTable(
                name: "Matieres",
                columns: table => new
                {
                    IDMatiere = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NomMatiere = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    PrixStandardMensuel = table.Column<double>(type: "REAL", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    EstArchivee = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matieres", x => x.IDMatiere);
                });

            migrationBuilder.CreateTable(
                name: "Professeurs",
                columns: table => new
                {
                    IDProfesseur = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Telephone = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    EstArchive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Professeurs", x => x.IDProfesseur);
                });

            migrationBuilder.CreateTable(
                name: "Salles",
                columns: table => new
                {
                    IDSalle = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NomOuNumeroSalle = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Capacite = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    EstArchivee = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salles", x => x.IDSalle);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    IDUtilisateur = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Login = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MotDePasseHashe = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false),
                    NomComplet = table.Column<string>(type: "TEXT", nullable: false),
                    EstActif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.IDUtilisateur);
                });

            migrationBuilder.CreateTable(
                name: "Groupes",
                columns: table => new
                {
                    IDGroupe = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NomDescriptifGroupe = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IDMatiere = table.Column<int>(type: "INTEGER", nullable: false),
                    IDProfesseur = table.Column<int>(type: "INTEGER", nullable: false),
                    Niveau = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    EstArchive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groupes", x => x.IDGroupe);
                    table.ForeignKey(
                        name: "FK_Groupes_Matieres_IDMatiere",
                        column: x => x.IDMatiere,
                        principalTable: "Matieres",
                        principalColumn: "IDMatiere",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Groupes_Professeurs_IDProfesseur",
                        column: x => x.IDProfesseur,
                        principalTable: "Professeurs",
                        principalColumn: "IDProfesseur",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProfesseurMatieres",
                columns: table => new
                {
                    IDProfesseurMatiere = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IDProfesseur = table.Column<int>(type: "INTEGER", nullable: false),
                    IDMatiere = table.Column<int>(type: "INTEGER", nullable: false),
                    PourcentageRemuneration = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfesseurMatieres", x => x.IDProfesseurMatiere);
                    table.ForeignKey(
                        name: "FK_ProfesseurMatieres_Matieres_IDMatiere",
                        column: x => x.IDMatiere,
                        principalTable: "Matieres",
                        principalColumn: "IDMatiere",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProfesseurMatieres_Professeurs_IDProfesseur",
                        column: x => x.IDProfesseur,
                        principalTable: "Professeurs",
                        principalColumn: "IDProfesseur",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Paiements",
                columns: table => new
                {
                    IDPaiement = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IDEtudiant = table.Column<int>(type: "INTEGER", nullable: false),
                    IDUtilisateurEnregistrement = table.Column<int>(type: "INTEGER", nullable: false),
                    DatePaiement = table.Column<string>(type: "TEXT", nullable: false),
                    MontantTotalRecuTransaction = table.Column<double>(type: "REAL", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paiements", x => x.IDPaiement);
                    table.ForeignKey(
                        name: "FK_Paiements_Etudiants_IDEtudiant",
                        column: x => x.IDEtudiant,
                        principalTable: "Etudiants",
                        principalColumn: "IDEtudiant",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Paiements_Utilisateurs_IDUtilisateurEnregistrement",
                        column: x => x.IDUtilisateurEnregistrement,
                        principalTable: "Utilisateurs",
                        principalColumn: "IDUtilisateur",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inscriptions",
                columns: table => new
                {
                    IDInscription = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IDEtudiant = table.Column<int>(type: "INTEGER", nullable: false),
                    IDGroupe = table.Column<int>(type: "INTEGER", nullable: false),
                    DateInscription = table.Column<string>(type: "TEXT", nullable: false),
                    PrixConvenuMensuel = table.Column<double>(type: "REAL", nullable: false),
                    JourEcheanceMensuelle = table.Column<int>(type: "INTEGER", nullable: false),
                    EstActif = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateDesinscription = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscriptions", x => x.IDInscription);
                    table.ForeignKey(
                        name: "FK_Inscriptions_Etudiants_IDEtudiant",
                        column: x => x.IDEtudiant,
                        principalTable: "Etudiants",
                        principalColumn: "IDEtudiant",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inscriptions_Groupes_IDGroupe",
                        column: x => x.IDGroupe,
                        principalTable: "Groupes",
                        principalColumn: "IDGroupe",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SeancesPlanning",
                columns: table => new
                {
                    IDSeance = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IDGroupe = table.Column<int>(type: "INTEGER", nullable: false),
                    IDSalle = table.Column<int>(type: "INTEGER", nullable: false),
                    DateSeance = table.Column<string>(type: "TEXT", nullable: false),
                    HeureDebut = table.Column<string>(type: "TEXT", nullable: false),
                    HeureFin = table.Column<string>(type: "TEXT", nullable: false),
                    StatutSeance = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeancesPlanning", x => x.IDSeance);
                    table.ForeignKey(
                        name: "FK_SeancesPlanning_Groupes_IDGroupe",
                        column: x => x.IDGroupe,
                        principalTable: "Groupes",
                        principalColumn: "IDGroupe",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeancesPlanning_Salles_IDSalle",
                        column: x => x.IDSalle,
                        principalTable: "Salles",
                        principalColumn: "IDSalle",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetailsPaiements",
                columns: table => new
                {
                    IDDetailPaiement = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IDPaiement = table.Column<int>(type: "INTEGER", nullable: false),
                    IDInscription = table.Column<int>(type: "INTEGER", nullable: false),
                    AnneeMoisConcerne = table.Column<string>(type: "TEXT", nullable: false),
                    MontantPayePourEcheance = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailsPaiements", x => x.IDDetailPaiement);
                    table.ForeignKey(
                        name: "FK_DetailsPaiements_Inscriptions_IDInscription",
                        column: x => x.IDInscription,
                        principalTable: "Inscriptions",
                        principalColumn: "IDInscription",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetailsPaiements_Paiements_IDPaiement",
                        column: x => x.IDPaiement,
                        principalTable: "Paiements",
                        principalColumn: "IDPaiement",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetailsPaiements_IDInscription",
                table: "DetailsPaiements",
                column: "IDInscription");

            migrationBuilder.CreateIndex(
                name: "IX_DetailsPaiements_IDPaiement",
                table: "DetailsPaiements",
                column: "IDPaiement");

            migrationBuilder.CreateIndex(
                name: "IX_Etudiants_Telephone",
                table: "Etudiants",
                column: "Telephone",
                unique: true,
                filter: "[Telephone] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Groupes_IDMatiere",
                table: "Groupes",
                column: "IDMatiere");

            migrationBuilder.CreateIndex(
                name: "IX_Groupes_IDProfesseur",
                table: "Groupes",
                column: "IDProfesseur");

            migrationBuilder.CreateIndex(
                name: "IX_Groupes_NomDescriptifGroupe",
                table: "Groupes",
                column: "NomDescriptifGroupe",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inscriptions_IDEtudiant_IDGroupe",
                table: "Inscriptions",
                columns: new[] { "IDEtudiant", "IDGroupe" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inscriptions_IDGroupe",
                table: "Inscriptions",
                column: "IDGroupe");

            migrationBuilder.CreateIndex(
                name: "IX_Matieres_NomMatiere",
                table: "Matieres",
                column: "NomMatiere",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_IDEtudiant",
                table: "Paiements",
                column: "IDEtudiant");

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_IDUtilisateurEnregistrement",
                table: "Paiements",
                column: "IDUtilisateurEnregistrement");

            migrationBuilder.CreateIndex(
                name: "IX_ProfesseurMatieres_IDMatiere",
                table: "ProfesseurMatieres",
                column: "IDMatiere");

            migrationBuilder.CreateIndex(
                name: "IX_ProfesseurMatieres_IDProfesseur_IDMatiere",
                table: "ProfesseurMatieres",
                columns: new[] { "IDProfesseur", "IDMatiere" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Professeurs_Telephone",
                table: "Professeurs",
                column: "Telephone",
                unique: true,
                filter: "[Telephone] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Salles_NomOuNumeroSalle",
                table: "Salles",
                column: "NomOuNumeroSalle",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeancesPlanning_IDGroupe_DateSeance_HeureDebut",
                table: "SeancesPlanning",
                columns: new[] { "IDGroupe", "DateSeance", "HeureDebut" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeancesPlanning_IDSalle_DateSeance_HeureDebut",
                table: "SeancesPlanning",
                columns: new[] { "IDSalle", "DateSeance", "HeureDebut" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Login",
                table: "Utilisateurs",
                column: "Login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetailsPaiements");

            migrationBuilder.DropTable(
                name: "ProfesseurMatieres");

            migrationBuilder.DropTable(
                name: "SeancesPlanning");

            migrationBuilder.DropTable(
                name: "Inscriptions");

            migrationBuilder.DropTable(
                name: "Paiements");

            migrationBuilder.DropTable(
                name: "Salles");

            migrationBuilder.DropTable(
                name: "Groupes");

            migrationBuilder.DropTable(
                name: "Etudiants");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Matieres");

            migrationBuilder.DropTable(
                name: "Professeurs");
        }
    }
}
