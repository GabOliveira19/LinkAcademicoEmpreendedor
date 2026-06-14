using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkAcademicoEmpreendedor.Migrations
{
    /// <inheritdoc />
    public partial class AddConfiguracoesVisuaisUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracoesVisuaisUsuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    TipoUsuario = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Tema = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TamanhoFonte = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Densidade = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReduzirAnimacoes = table.Column<bool>(type: "bit", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesVisuaisUsuario", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracoesVisuaisUsuario_UsuarioId_TipoUsuario",
                table: "ConfiguracoesVisuaisUsuario",
                columns: new[] { "UsuarioId", "TipoUsuario" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracoesVisuaisUsuario");
        }
    }
}
