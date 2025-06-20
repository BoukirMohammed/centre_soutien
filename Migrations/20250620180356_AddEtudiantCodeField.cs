using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace centre_soutien.Migrations
{
    /// <inheritdoc />
    public partial class AddEtudiantCodeField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ajouter la colonne Code
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Etudiants",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // Générer des codes temporaires pour les étudiants existants
            migrationBuilder.Sql(@"
                UPDATE Etudiants 
                SET Code = 'ETU' || printf('%04d', IDEtudiant) 
                WHERE Code = '' OR Code IS NULL;
            ");

            // Créer l'index unique
            migrationBuilder.CreateIndex(
                name: "IX_Etudiants_Code_Unique",
                table: "Etudiants",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Supprimer l'index
            migrationBuilder.DropIndex(
                name: "IX_Etudiants_Code_Unique",
                table: "Etudiants");

            // Supprimer la colonne
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Etudiants");
        }
    }
}