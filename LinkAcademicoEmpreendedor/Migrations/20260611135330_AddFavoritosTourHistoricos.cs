using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkAcademicoEmpreendedor.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoritosTourHistoricos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TourInicialConcluido",
                table: "Empresas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TourInicialConcluido",
                table: "Alunos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "FavoritosTalentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoritosTalentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoritosTalentos_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FavoritosTalentos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FavoritosVagas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    OportunidadeId = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoritosVagas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoritosVagas_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoritosVagas_Oportunidades_OportunidadeId",
                        column: x => x.OportunidadeId,
                        principalTable: "Oportunidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoritosTalentos_AlunoId",
                table: "FavoritosTalentos",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoritosTalentos_EmpresaId_AlunoId",
                table: "FavoritosTalentos",
                columns: new[] { "EmpresaId", "AlunoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoritosVagas_AlunoId_OportunidadeId",
                table: "FavoritosVagas",
                columns: new[] { "AlunoId", "OportunidadeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoritosVagas_OportunidadeId",
                table: "FavoritosVagas",
                column: "OportunidadeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoritosTalentos");

            migrationBuilder.DropTable(
                name: "FavoritosVagas");

            migrationBuilder.DropColumn(
                name: "TourInicialConcluido",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "TourInicialConcluido",
                table: "Alunos");
        }
    }
}
