using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace centre_soutien.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureSeancePlanningNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "SeancesPlanning",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "SeancesPlanning");
        }
    }
}
