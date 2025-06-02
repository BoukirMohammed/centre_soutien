using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace centre_soutien.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeancePlanningForRecurrenceV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Seances_Groupe_Date_HeureDebut_Unique",
                table: "SeancesPlanning");

            migrationBuilder.DropIndex(
                name: "IX_Seances_Salle_Date_HeureDebut_Unique",
                table: "SeancesPlanning");

            migrationBuilder.DropColumn(
                name: "StatutSeance",
                table: "SeancesPlanning");

            migrationBuilder.RenameColumn(
                name: "DateSeance",
                table: "SeancesPlanning",
                newName: "DateDebutValidite");

            migrationBuilder.AddColumn<string>(
                name: "DateFinValidite",
                table: "SeancesPlanning",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstActif",
                table: "SeancesPlanning",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "JourSemaine",
                table: "SeancesPlanning",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Seances_Groupe_Jour_HeureDebut_Unique",
                table: "SeancesPlanning",
                columns: new[] { "IDGroupe", "JourSemaine", "HeureDebut" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seances_Salle_Jour_HeureDebut_Unique",
                table: "SeancesPlanning",
                columns: new[] { "IDSalle", "JourSemaine", "HeureDebut" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Seances_Groupe_Jour_HeureDebut_Unique",
                table: "SeancesPlanning");

            migrationBuilder.DropIndex(
                name: "IX_Seances_Salle_Jour_HeureDebut_Unique",
                table: "SeancesPlanning");

            migrationBuilder.DropColumn(
                name: "DateFinValidite",
                table: "SeancesPlanning");

            migrationBuilder.DropColumn(
                name: "EstActif",
                table: "SeancesPlanning");

            migrationBuilder.DropColumn(
                name: "JourSemaine",
                table: "SeancesPlanning");

            migrationBuilder.RenameColumn(
                name: "DateDebutValidite",
                table: "SeancesPlanning",
                newName: "DateSeance");

            migrationBuilder.AddColumn<string>(
                name: "StatutSeance",
                table: "SeancesPlanning",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "Planifiée");

            migrationBuilder.CreateIndex(
                name: "IX_Seances_Groupe_Date_HeureDebut_Unique",
                table: "SeancesPlanning",
                columns: new[] { "IDGroupe", "DateSeance", "HeureDebut" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seances_Salle_Date_HeureDebut_Unique",
                table: "SeancesPlanning",
                columns: new[] { "IDSalle", "DateSeance", "HeureDebut" },
                unique: true);
        }
    }
}
