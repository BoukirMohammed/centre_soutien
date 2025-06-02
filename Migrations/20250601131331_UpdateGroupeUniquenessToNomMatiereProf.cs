using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace centre_soutien.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGroupeUniquenessToNomMatiereProf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Groupes_NomDescriptifGroupe",
                table: "Groupes");

            migrationBuilder.CreateIndex(
                name: "IX_Groupes_Nom_Matiere_Prof_Unique",
                table: "Groupes",
                columns: new[] { "NomDescriptifGroupe", "IDMatiere", "IDProfesseur" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Groupes_Nom_Matiere_Prof_Unique",
                table: "Groupes");

            migrationBuilder.CreateIndex(
                name: "IX_Groupes_NomDescriptifGroupe",
                table: "Groupes",
                column: "NomDescriptifGroupe",
                unique: true);
        }
    }
}
