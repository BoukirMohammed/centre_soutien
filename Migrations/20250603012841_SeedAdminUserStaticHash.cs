using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace centre_soutien.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUserStaticHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Utilisateurs",
                keyColumn: "IDUtilisateur",
                keyValue: 1,
                column: "MotDePasseHashe",
                value: "nlJFt6/bDjrGLBcd/oJoeQ==;100000;xhvXeJWxAJ/AinHfJbYjDkQAtafY6NjGmgejFvxCL4I=");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Utilisateurs",
                keyColumn: "IDUtilisateur",
                keyValue: 1,
                column: "MotDePasseHashe",
                value: "23uFhfRjdM0NkaNdq04JcA==;100000;G18SulkjszW9pFN6zEku0u26OmAtOaReXI+MgxhQ4yY=");
        }
    }
}
