using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace centre_soutien.Migrations
{
    /// <inheritdoc />
    public partial class AddInscriptionEntityAndRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Inscriptions_IDEtudiant_IDGroupe",
                table: "Inscriptions",
                newName: "IX_Inscriptions_Etudiant_Groupe_Unique");

            migrationBuilder.AlterColumn<string>(
                name: "DateDesinscription",
                table: "Inscriptions",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Inscriptions_Etudiant_Groupe_Unique",
                table: "Inscriptions",
                newName: "IX_Inscriptions_IDEtudiant_IDGroupe");

            migrationBuilder.AlterColumn<string>(
                name: "DateDesinscription",
                table: "Inscriptions",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
