using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace centre_soutien.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureSeancePlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "SeancesPlanning");

            migrationBuilder.RenameIndex(
                name: "IX_SeancesPlanning_IDSalle_DateSeance_HeureDebut",
                table: "SeancesPlanning",
                newName: "IX_Seances_Salle_Date_HeureDebut_Unique");

            migrationBuilder.RenameIndex(
                name: "IX_SeancesPlanning_IDGroupe_DateSeance_HeureDebut",
                table: "SeancesPlanning",
                newName: "IX_Seances_Groupe_Date_HeureDebut_Unique");

            migrationBuilder.AlterColumn<string>(
                name: "StatutSeance",
                table: "SeancesPlanning",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "Planifiée",
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Seances_Salle_Date_HeureDebut_Unique",
                table: "SeancesPlanning",
                newName: "IX_SeancesPlanning_IDSalle_DateSeance_HeureDebut");

            migrationBuilder.RenameIndex(
                name: "IX_Seances_Groupe_Date_HeureDebut_Unique",
                table: "SeancesPlanning",
                newName: "IX_SeancesPlanning_IDGroupe_DateSeance_HeureDebut");

            migrationBuilder.AlterColumn<string>(
                name: "StatutSeance",
                table: "SeancesPlanning",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldDefaultValue: "Planifiée");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "SeancesPlanning",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
