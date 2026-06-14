using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkAcademicoEmpreendedor.Migrations
{
    /// <inheritdoc />
    public partial class AddAcessibilidadeConfiguracoesVisuais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ModoDaltonico",
                table: "ConfiguracoesVisuaisUsuario",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ReduzirCores",
                table: "ConfiguracoesVisuaisUsuario",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModoDaltonico",
                table: "ConfiguracoesVisuaisUsuario");

            migrationBuilder.DropColumn(
                name: "ReduzirCores",
                table: "ConfiguracoesVisuaisUsuario");
        }
    }
}
